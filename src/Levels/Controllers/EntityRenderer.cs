using System.Collections.Generic;
using System.Linq;
using Game.Core;
using Game.Core.ECS;
using Game.Core.Services;
using Game.Models;

namespace Game.Levels.Controllers;

public partial class EntityRenderer : Node
{
	[Export]
	private EntityComponentStore _entities = null!;

	[Export]
	private PackedScene _multiMesh = null!;

	[Export]
	private float _renderDistanceFactor = 1.2f;

	private const int GridSize = 64;

	// Only for checking visibility. Nothing stored inside
	private CenteredMovingUniformGrid<object> _visibilityGrid = null!;
	private readonly Dictionary<MultiMeshInstance2D, int> _mmiVisibilityCount = [];

	private readonly Dictionary<string, MultiMeshInstance2D> _spriteToMultiMesh = [];

	private const int _initialInstanceCount = 2000;
	private const float _instanceGrowthMultiplier = 1.5f;

	private Vector2 PlayerPosition =>
		GameWorld.Instance.MainPlayer?.GlobalPosition ?? Vector2.Zero;

	public override void _Ready()
	{
		var viewport = GetViewport();
		if (viewport is null)
			return;

		var windowSize = viewport.GetVisibleRect().Size * _renderDistanceFactor;
		_visibilityGrid = new CenteredMovingUniformGrid<object>(
			GridSize,
			windowSize
		);

		// Render last to allow other systems to do their work first
		ProcessPriority = 16;
	}

	public override void _Process(double delta)
	{
		_visibilityGrid.Recenter(PlayerPosition);
		foreach (var mmi in _mmiVisibilityCount.Keys)
		{
			_mmiVisibilityCount[mmi] = 0;
			mmi.Multimesh.VisibleInstanceCount = 0;
		}

		foreach (var (id, pos, sprite) in _entities.Query<PositionComponent, AnimatedSpriteComponent>())
		{
			if (!_visibilityGrid.ContainsWorld(pos.Position))
				continue;

			if (!_spriteToMultiMesh.TryGetValue(sprite.SpriteName, out var mmi))
				mmi = CreateNewMultiMesh(sprite.SpriteName);

			var count = _mmiVisibilityCount[mmi]++;
			if (count >= mmi.Multimesh.InstanceCount)
			{
				mmi.Multimesh.InstanceCount = (int)(mmi.Multimesh.InstanceCount * _instanceGrowthMultiplier);
			}

			UpdateEnemyInstance(mmi, id, count, pos.Position, sprite);
		}

		foreach (var (mmi, visibleCount) in _mmiVisibilityCount)
		{
			mmi.Multimesh.VisibleInstanceCount = visibleCount;
		}
	}

	private void UpdateEnemyInstance(
		MultiMeshInstance2D mmi,
		int entityId,
		int instanceIdx,
		Vector2 pos,
		AnimatedSpriteComponent sprite
	)
	{
		var multiMesh = mmi.Multimesh;
		bool flip;

		var player = GameWorld.Instance.MainPlayer;
		if (player is null)
			flip = false;
		else
			flip = player.GlobalPosition < pos;

		multiMesh.SetInstanceTransform2D(
			instanceIdx,
			new Transform2D(0, pos)
		);

		var updatedSprite = sprite;
		if (updatedSprite.AnimationTime <= 0)
		{
			// TODO: Will continue into empty sprite frame tiles due to
			// AnimatedSpriteComponent not tracking which XY index in the last 
			// tile in the sprite frame
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
		_mmiVisibilityCount.TryAdd(mmi, 0);
		return mmi;
	}
}
