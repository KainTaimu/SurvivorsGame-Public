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
	private float _renderDistanceFactor = 1.1f;

	private const int GridSize = 64;

	private CenteredMovingUniformGrid<string> _visibilityGrid = null!;
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
		_visibilityGrid = new CenteredMovingUniformGrid<string>(
			GridSize,
			windowSize
		);

		// Render last to allow other systems to do their work first
		ProcessPriority = 16;

		_entities.BeforeEntityUnregistered += BeforeEntityUnregistered;
	}

	public override void _Process(double delta)
	{
		// calculate vis grid
		_visibilityGrid.Recenter(PlayerPosition);
		foreach (var mmi in _mmiVisibilityCount.Keys)
			_mmiVisibilityCount[mmi] = 0;

		var vis = AddObjectsToGrid();
		foreach (var (key, val) in vis)
		{
			if (!_spriteToMultiMesh.TryGetValue(key, out var mmi))
				continue;
			if (Engine.GetProcessFrames() % 10 == 0)
				Logger.LogDebug(key, val);
			mmi.Multimesh.VisibleInstanceCount = val;
		}

		foreach (var (id, pos, sprite) in _entities.Query<PositionComponent, AnimatedSpriteComponent>())
		{
			if (!_visibilityGrid.ContainsWorld(pos.Position))
				continue;

			if (!_spriteToMultiMesh.TryGetValue(sprite.SpriteName, out var mmi))
				mmi = CreateNewMultiMesh(sprite.SpriteName);

			var count = ++_mmiVisibilityCount[mmi];
			if (count > mmi.Multimesh.InstanceCount)
			{
				mmi.Multimesh.InstanceCount = (int)(mmi.Multimesh.InstanceCount * _instanceGrowthMultiplier);
			}
			UpdateEnemyInstance(mmi, id, count, pos.Position, sprite);
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

	private void BeforeEntityUnregistered(int entityId)
	{
		throw new NotImplementedException("TODO");
	}

	private Dictionary<string, int> AddObjectsToGrid()
	{
		var vis = new Dictionary<string, int>();
		foreach (var (_, pos, sprite) in _entities.Query<PositionComponent, AnimatedSpriteComponent>())
		{
			if (!_visibilityGrid.ContainsWorld(pos.Position))
				continue;

			var cell = _visibilityGrid.GetCellWorld(pos.Position);
			if (cell is null)
				continue;
			cell.Add(sprite.SpriteName);

			if (!vis.TryAdd(sprite.SpriteName, 1))
				vis[sprite.SpriteName]++;
		}
		return vis;
	}
}
