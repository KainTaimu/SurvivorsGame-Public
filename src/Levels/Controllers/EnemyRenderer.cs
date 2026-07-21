using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using Game.Core.ECS;
using Game.Core.Extensions;
using Game.Core.Services;

namespace Game.Levels.Controllers;

public partial class EnemyRenderer : Node
{
	[Export]
	private PackedScene _multiMesh = null!;

	[Export]
	private float RenderDistanceFactor
	{
		get;
		set
		{
			field = value;
			CenterVisibilityGrid();
		}
	} = 1.2f;

	private Rect2 _visibilityRect;
	private readonly Dictionary<MultiMesh, int> _mmiVisibilityCount = [];

	private readonly Dictionary<string, MultiMesh> _spriteToMultiMesh = [];

	private const int INITIAL_INSTANCE_COUNT = 2000;
	private const float INSTANCE_GROWTH_MULTIPLIER = 1.5f;

	private Vector2 PlayerPosition => GameWorld.Instance.MainPlayer.GlobalPosition;

	public double ProcessTime { get; private set; }

	private Viewport? _cachedViewport;

	public override void _Ready()
	{
		_cachedViewport = GetViewport();
		CenterVisibilityGrid();
		// Render last to allow other systems to do their work first
		ProcessPriority = 16;
	}

	private void CenterVisibilityGrid()
	{
		_cachedViewport ??= GetViewport();
		if (_cachedViewport is null)
			return;

		var scale = 1f / _cachedViewport.GetCamera2D().Zoom.GetLargestComponent();
		_visibilityRect = _cachedViewport
			.GetVisibleRect()
			.GetCenteredToPoint(PlayerPosition, scale * RenderDistanceFactor);
	}

	public override void _Process(double delta)
	{
		CenterVisibilityGrid();
		var start = Time.GetTicksMsec();
		foreach (var mmi in _mmiVisibilityCount.Keys)
		{
			_mmiVisibilityCount[mmi] = 0;
			mmi.VisibleInstanceCount = 0;
		}

		UpdateEnemySpritesQuery(GameWorld.World);

		foreach (var (mmi, visibleCount) in _mmiVisibilityCount)
			mmi.VisibleInstanceCount = visibleCount;
		ProcessTime = Time.GetTicksMsec() - start;
	}

	[Query]
	[All<PositionComponent, AnimatedSpriteComponent>]
	[Any<VelocityComponent>]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void UpdateEnemySprites(in Entity entity, ref PositionComponent pos, ref AnimatedSpriteComponent sprite)
	{
		if (!GameWorld.World.IsAlive(entity))
			return;
		if (!_visibilityRect.HasPoint(pos.Position))
			return;

		if (!_spriteToMultiMesh.TryGetValue(sprite.SpriteName, out var mmi))
			mmi = CreateNewMultiMesh(sprite.SpriteName);

		var count = _mmiVisibilityCount[mmi]++;
		if (count >= mmi.InstanceCount)
			mmi.InstanceCount = (int)(mmi.InstanceCount * INSTANCE_GROWTH_MULTIPLIER);

		if (GameWorld.World.Has<VelocityComponent>(entity))
		{
			var vel = GameWorld.World.Get<VelocityComponent>(entity);
			UpdateEnemyInstance(mmi, count, pos.Position, ref sprite, vel.Velocity.X < 0);
		}
		else
			UpdateEnemyInstance(mmi, count, pos.Position, ref sprite);
	}

	private void UpdateEnemyInstance(
		MultiMesh multiMesh,
		int instanceIdx,
		Vector2 pos,
		ref AnimatedSpriteComponent sprite,
		bool? flip = null
	)
	{
		flip ??= PlayerPosition < pos;

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
			flip: flip.Value,
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
