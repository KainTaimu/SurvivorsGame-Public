using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using Game.Core.ECS;
using Game.Core.Services;
using Game.Models;

namespace Game.Levels.Controllers;

public partial class EnemyRenderer : Node
{
	[Export]
	private PackedScene _multiMesh = null!;

	[Export]
	private float _renderDistanceFactor = 1.2f;

	// Only for checking visibility. Nothing stored inside
	private CenteredMovingUniformGrid<object> _visibilityGrid = null!;
	private readonly Dictionary<MultiMesh, int> _mmiVisibilityCount = [];

	private readonly Dictionary<string, MultiMesh> _spriteToMultiMesh = [];

	private const int GRID_SIZE = 64;
	private const int INITIAL_INSTANCE_COUNT = 2000;
	private const float INSTANCE_GROWTH_MULTIPLIER = 1.5f;

	private Vector2 PlayerPosition => GameWorld.Instance.MainPlayer.GlobalPosition;

	public override void _Ready()
	{
		var viewport = GetViewport();
		if (viewport is null)
			return;

		var windowSize = viewport.GetVisibleRect().Size * _renderDistanceFactor;
		_visibilityGrid = new CenteredMovingUniformGrid<object>(GRID_SIZE, windowSize);

		// Render last to allow other systems to do their work first
		ProcessPriority = 16;
	}

	public override void _Process(double delta)
	{
		_visibilityGrid.Recenter(PlayerPosition);
		foreach (var mmi in _mmiVisibilityCount.Keys)
		{
			_mmiVisibilityCount[mmi] = 0;
			mmi.VisibleInstanceCount = 0;
		}

		UpdateEnemySpritesQuery(GameWorld.World);

		foreach (var (mmi, visibleCount) in _mmiVisibilityCount)
			mmi.VisibleInstanceCount = visibleCount;
	}

	[Query]
	[All<PositionComponent, AnimatedSpriteComponent>]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void UpdateEnemySprites(ref PositionComponent pos, ref AnimatedSpriteComponent sprite)
	{
		if (!_visibilityGrid.ContainsWorld(pos.Position))
			return;

		if (!_spriteToMultiMesh.TryGetValue(sprite.SpriteName, out var mmi))
			mmi = CreateNewMultiMesh(sprite.SpriteName);

		var count = _mmiVisibilityCount[mmi]++;
		if (count >= mmi.InstanceCount)
			mmi.InstanceCount = (int)(mmi.InstanceCount * INSTANCE_GROWTH_MULTIPLIER);

		UpdateEnemyInstance(mmi, count, pos.Position, ref sprite);
	}

	private void UpdateEnemyInstance(
		MultiMesh multiMesh,
		int instanceIdx,
		Vector2 pos,
		ref AnimatedSpriteComponent sprite
	)
	{
		var flip = PlayerPosition < pos;

		if (sprite.AnimationTime <= 0)
		{
			// TODO: Will continue into empty sprite frame tiles due to
			// AnimatedSpriteComponent not tracking which XY index in the last
			// tile in the sprite frame
			sprite.AnimationTime = sprite.AnimationSpeed;
			if (sprite.FrameIdxX + 1 >= sprite.FrameCountX)
				sprite.FrameIdxX = 0;
			else
				sprite.FrameIdxX++;
		}
		else
			sprite.AnimationTime -= GetProcessDeltaTime();

		var custom = new EnemyShaderCustomData(
			sprite.FrameCountX,
			sprite.FrameCountY,
			sprite.FrameSizePxX,
			sprite.FrameSizePxY,
			sprite.FrameIdxX,
			flip: flip,
			opacity: sprite.Opacity,
			flash: sprite.Flash,
			scale: (byte)Mathf.Clamp((sprite.Scale - 0.5f) / 4.5f * 255f, 0, 255)
		);
		var data = new Color(custom.R, custom.G, custom.B, custom.A);

		multiMesh.SetInstanceCustomData(instanceIdx, data);
		multiMesh.SetInstanceTransform2D(instanceIdx, new Transform2D(0, pos));
	}

	private MultiMesh CreateNewMultiMesh(string spriteName)
	{
		var mmi = _multiMesh.Instantiate<MultiMeshInstance2D>();
		// To avoid flickering, pre-initialize _initialInstanceCount instances
		mmi.Multimesh.InstanceCount = INITIAL_INSTANCE_COUNT;
		mmi.Multimesh.VisibleInstanceCount = 0;

		var ss = ServiceLocator.GetService<SpriteFrameMappingsService>();
		if (ss is null)
		{
			Logger.LogError("Could not get SpriteFrameMappingsService.");
			mmi.Texture = new PlaceholderTexture2D { Size = new Vector2(32, 32) };
			_spriteToMultiMesh.Add(spriteName, mmi.Multimesh);
			_mmiVisibilityCount.TryAdd(mmi.Multimesh, 0);
			return mmi.Multimesh;
		}

		mmi.Texture = ss.GetSpriteFrame(spriteName);

		AddChild(mmi);
		_spriteToMultiMesh.Add(spriteName, mmi.Multimesh);
		_mmiVisibilityCount.TryAdd(mmi.Multimesh, 0);
		return mmi.Multimesh;
	}
}
