using System.Collections.Generic;
using Game.Core;
using Game.Core.ECS;
using Game.Core.Services;
using Game.Models;

namespace Game.Levels.Controllers;

public class EnemyShaderCustomData(
	byte frameX,
	byte frameY,
	byte frameSizePxX,
	byte frameSizePxY,
	byte frameIdxX = 0,
	byte frameIdxY = 0,
	bool flip = false,
	byte opacity = 255,
	byte flash = 0
)
{
	public float R => GetR();
	public float G => GetG();
	public float B => GetB();
	public float A => GetA();

	// Channel R
	public bool Flip = flip; // 1 bit
	public byte Opacity = opacity; // 2 byte = 8 bit
	public byte Flash = flash;
	private const int FlipPosition = 0;
	private const int OpacityPosition = 1;
	private const int FlashPosition = 9;

	// Channel B
	public readonly byte FrameX = frameX;
	public readonly byte FrameY = frameY;
	public readonly byte FrameIdxX = frameIdxX;
	public readonly byte FrameIdxY = frameIdxY;
	private const int FrameXPosition = 0;
	private const int FrameYPosition = 8;
	private const int FrameIdxXPosition = 16;
	private const int FrameIdxYPosition = 24;

	// Channel A
	public readonly byte FrameSizePxX = frameSizePxX;
	public readonly byte FrameSizePxY = frameSizePxY;
	private const int FrameSizePxXPosition = 0;
	private const int FrameSizePxYPosition = 8;

	private float GetR()
	{
		var bits = 0u;

		bits ^= (Flip ? 1u : 0u) << FlipPosition;
		bits ^= (uint)Opacity << OpacityPosition;
		bits ^= (uint)Flash << FlashPosition;

		return BitConverter.UInt32BitsToSingle(bits);
	}

	// TODO: What to use channel G for
	private float GetG()
	{
		return 0f;
	}

	private float GetB()
	{
		var bits = 0u;

		bits ^= (uint)FrameX << FrameXPosition;
		bits ^= (uint)FrameY << FrameYPosition;
		bits ^= (uint)FrameIdxX << FrameIdxXPosition;
		bits ^= (uint)FrameIdxY << FrameIdxYPosition;

		return BitConverter.UInt32BitsToSingle(bits);
	}

	private float GetA()
	{
		var bits = 0u;

		bits ^= (uint)FrameSizePxX << FrameSizePxXPosition;
		bits ^= (uint)FrameSizePxY << FrameSizePxYPosition;

		return BitConverter.UInt32BitsToSingle(bits);
	}
}

public partial class EntityRenderer : Node
{
	[Export]
	private EntityComponentStore _entities = null!;

	[Export]
	private PackedScene _multiMesh = null!;

	private const int GridSize = 128;
	private CenteredMovingUniformGrid<(Vector2, int)> _grid = null!;

	private readonly Dictionary<
		string,
		MultiMeshInstance2D
	> _spriteToMultiMesh = [];
	private readonly Dictionary<int, int> _idToInstanceIndex = [];
	private int _visibleCount;

	private const int _initialInstanceCount = 2000;
	private const float _instanceGrowthMultiplier = 1.5f;

	private Vector2 PlayerPosition =>
		GameWorld.Instance.MainPlayer?.GlobalPosition ?? Vector2.Zero;

	public override void _Ready()
	{
		var viewport = GetViewport();
		if (viewport is null)
			return;

		var windowSize = viewport.GetVisibleRect().Size * 1.2f;
		_grid = new CenteredMovingUniformGrid<(Vector2, int)>(
			GridSize,
			windowSize
		);

		// Render last to allow other systems to do their work first
		ProcessPriority = 16;

		_entities.BeforeEntityUnregistered += BeforeEntityUnregistered;
	}

	public override void _Process(double delta)
	{
		// TODO:
		// Instead of moving hidden instances to a large Vector2 position,
		// Change MultiMesh.VisibleInstanceCount based on how many entities
		// are on the screen.
		_grid.Recenter(PlayerPosition);
		// _grid.ClearGrid();
		// AddObjectsToGrid();

		foreach (
			var (id, sprite, pos, type) in _entities.Query<
				AnimatedSpriteComponent,
				PositionComponent,
				EntityTypeComponent
			>()
		)
		{
			if (!_spriteToMultiMesh.TryGetValue(sprite.SpriteName, out var mmi))
			{
				mmi = CreateNewMultiMesh(sprite.SpriteName);
			}
			if (!_idToInstanceIndex.TryGetValue(id, out var instanceIdx))
			{
				_idToInstanceIndex.Add(id, mmi.Multimesh.VisibleInstanceCount);
				instanceIdx = mmi.Multimesh.VisibleInstanceCount;
				mmi.Multimesh.VisibleInstanceCount++;

				if (
					mmi.Multimesh.VisibleInstanceCount
					>= mmi.Multimesh.InstanceCount - 10
				)
				{
					mmi.Multimesh.InstanceCount = (int)(
						mmi.Multimesh.InstanceCount * _instanceGrowthMultiplier
					);
					Logger.LogDebug(mmi.Multimesh.InstanceCount);
				}
			}

			switch (type.EntityType)
			{
				case EntityType.Enemy:
					UpdateEnemyInstance(
						mmi,
						id,
						instanceIdx,
						pos.Position,
						sprite
					);
					break;
			}
		}
	}

	private MultiMeshInstance2D CreateNewMultiMesh(string spriteName)
	{
		var mmi = _multiMesh.Instantiate<MultiMeshInstance2D>();
		// To avoid flickering, pre-initialize 1,000 instances and only make them visible when spawned
		mmi.Multimesh.InstanceCount = _initialInstanceCount;
		mmi.Multimesh.VisibleInstanceCount = 0;

		var ss = ServiceLocator.GetService<SpriteFrameMappingsService>();
		if (ss is null)
		{
			Logger.LogError("Could not get SpriteFrameMappingsService.");
			mmi.Texture = new PlaceholderTexture2D();
			return mmi;
		}

		mmi.Texture = ss.GetSpriteFrame(spriteName);

		AddChild(mmi);
		_spriteToMultiMesh.Add(spriteName, mmi);
		return mmi;
	}

	private void HideInstance(int instanceIdx) { }

	// NOTE: See NOTES.md for shader data layout
	private void UpdateEnemyInstance(
		MultiMeshInstance2D mmi,
		int entityId,
		int instanceIdx,
		Vector2 newPos,
		AnimatedSpriteComponent sprite
	)
	{
		var multiMesh = mmi.Multimesh;

		if (!_grid.ContainsWorld(newPos))
		{
			return;
		}

		bool flip;
		var player = GameWorld.Instance.MainPlayer;
		if (player is null)
			flip = false;
		else
			flip = player.GlobalPosition < newPos;

		multiMesh.SetInstanceTransform2D(
			instanceIdx,
			new Transform2D(0, newPos)
		);

		var updatedSprite = sprite;
		if (updatedSprite.AnimationTime <= 0)
		{
			updatedSprite.AnimationTime = updatedSprite.AnimationSpeed;
			if (updatedSprite.FrameIdxY + 1 >= updatedSprite.FrameCountY)
				updatedSprite.FrameIdxY = 0;
			else
				updatedSprite.FrameIdxY++;
		}
		else
		{
			updatedSprite.AnimationTime -= GetProcessDeltaTime();
		}

		_entities.UpdateComponent(entityId, updatedSprite);

		var custom = new EnemyShaderCustomData(
			updatedSprite.FrameCountX,
			updatedSprite.FrameCountY,
			updatedSprite.frameSizePxX,
			updatedSprite.frameSizePxY,
			frameIdxY: updatedSprite.FrameIdxY,
			flip: flip
		);
		var data = new Color(custom.R, custom.G, custom.B, custom.A);
		multiMesh.SetInstanceCustomData(instanceIdx, data);
	}

	private void BeforeEntityUnregistered(int id)
	{
		if (
			!_entities.GetComponent<AnimatedSpriteComponent>(id, out var sprite)
		)
		{
			Logger.LogDebug("sprite not found");
			return;
		}
		if (!_spriteToMultiMesh.TryGetValue(sprite.SpriteName, out var mmi))
		{
			Logger.LogDebug("multiMesh not found");
			return;
		}

		if (!_idToInstanceIndex.TryGetValue(id, out var idx))
		{
			Logger.LogDebug("instanceIdx not found");
			return;
		}

		var multiMesh = mmi.Multimesh;
		multiMesh.SetInstanceTransform2D(idx, new Transform2D(0, Vector2.Inf));

		_idToInstanceIndex.Remove(id);
	}

	private void AddObjectsToGrid()
	{
		foreach (var (id, pos) in _entities.Query<PositionComponent>())
		{
			if (!_grid.ContainsWorld(pos.Position))
				continue;

			var cell = _grid.GetCellWorld(pos.Position);
			cell?.Add((pos.Position, id));
		}
	}

	private int GetGridCount()
	{
		var i = 0;
		for (int x = 0; x < _grid.Dimensions.X; x++)
		{
			for (int y = 0; x < _grid.Dimensions.Y; y++)
			{
				var cell = _grid.GetCell(x, y)!;
				i += cell.Count;
			}
		}
		return i;
	}
}
