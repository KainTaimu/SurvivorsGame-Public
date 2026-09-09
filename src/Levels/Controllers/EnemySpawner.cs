using System.Collections.Generic;
using Arch.Core;
using CommunityToolkit.HighPerformance;
using Game.Core.ECS;
using Game.Core.Services;

namespace Game.Levels.Controllers;

public partial class EnemySpawner : Node
{
	public static EnemySpawner? Instance;

	public override void _Ready()
	{
		Instance = this;
	}

	public Entity? SpawnEnemy(EnemyBlueprint bp, in Vector2 position)
	{
		var ss = ServiceLocator.GetService<SpriteFrameMappingsService>();
		if (ss is null)
		{
			CustomLogger.LogError("Could not get sprite frame mappings service");
			return null;
		}

		var stats = bp.Stats;
		var spriteInfo = ss.GetSpriteInfo(bp.SpriteName);

		var entity = EnemyTracker.CreateEntity();
		var comps = new List<object>()
		{
			new EnemyTypeComponent(bp.Type),
			new HealthComponent(stats.MaxHealth),
			new PositionComponent { Position = position },
			new AnimatedSpriteComponent
			{
				SpriteName = spriteInfo?.SpriteName ?? "",
				AnimationSpeed = spriteInfo?.AnimationSpeed ?? Mathf.Inf,
				FrameCountX = spriteInfo?.FrameCountX ?? 1,
				FrameCountY = 1,
				FrameSizePxX = spriteInfo?.FrameSizePxX ?? 32,
				FrameSizePxY = spriteInfo?.FrameSizePxY ?? 32,
				Opacity = spriteInfo?.Opacity ?? 255,
				Flash = spriteInfo?.Flash ?? 0,
				Scale = bp.Stats.SpriteScaleMultiplier,
			},
			new CircleHitboxComponent(bp.Stats.SpriteScaleMultiplier * 16f),
			new MoveSpeedComponent(
				Mathf.CeilToInt(stats.MoveSpeed * stats.MoveSpeedMultiplier * GD.RandRange(0.9f, 1.1f)),
				stats.TurnSpeed
			),
			VelocityComponent.Zero,
			new EnemyContactDamageComponent(Mathf.CeilToInt(stats.DamageOnContact * stats.ContactDamageMultiplier)),
			new DeathRewardComponent(Mathf.CeilToInt(stats.MoneyDrop * stats.MoneyDropMultiplier)),
			new HitFeedbackComponent { HitTime = 0 },
			new CollisionLodComponent(CollisionLodLevel.Far),
			CollisionGpuIndexComponent.NotParticipating,
		};
		GameWorld.World.AddRange(entity, comps.AsSpan());

		foreach (var behavior in bp.EnemyBehaviors)
			behavior.AddComponent(entity);

		return entity;
	}
}