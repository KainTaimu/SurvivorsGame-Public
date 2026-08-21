using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using Game.Core;
using Game.Core.ECS;
using Game.Items.Projectiles;

namespace Game.Levels.Controllers;

public partial class EnemyDeathManager : Node
{
	[Signal]
	public delegate void OnEnemyDeathEventHandler(EntityObject entity);

	private readonly ConcurrentQueue<Entity> _pendingDeaths = [];

	private readonly EntityDeletionBuffer _deletionBuffer = new();

	public override void _Process(double delta)
	{
		UpdateNewDeathsQuery(GameWorld.World);
		UpdateDyingQuery(GameWorld.World, (float)delta);

		while (_pendingDeaths.TryDequeue(out var entity))
		{
			if (!GameWorld.World.IsAlive(entity))
				continue;
			GameWorld.World.Destroy(entity);
		}
	}

	[Query]
	[All<HealthComponent, PositionComponent>]
	[None<DyingMarkerComponent>]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract")]
	private void UpdateNewDeaths(Entity entity, ref HealthComponent health)
	{
		if (health.Health > 0)
			return;
		if (!GameWorld.World.IsAlive(entity))
			return;

		GameWorld.World.Add(entity, DyingMarkerComponent.Default);

		if (GameWorld.World.TryGet<DeathRewardComponent>(entity, out var reward))
			LevelData.Instance?.Money += reward.Money;

		EmitSignalOnEnemyDeath(new EntityObject(entity));
	}

	[Query]
	[All<DyingMarkerComponent, PositionComponent, VelocityComponent, AnimatedSpriteComponent>]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void UpdateDying(
		[Data] in float delta,
		Entity entity,
		ref DyingMarkerComponent dying,
		ref PositionComponent pos,
		ref VelocityComponent vel,
		ref AnimatedSpriteComponent spr
	)
	{
		if (dying.TimeLeftUntilDestroy <= 0)
		{
			_pendingDeaths.Enqueue(entity);
			return;
		}

		dying.TimeLeftUntilDestroy -= delta;
		pos.Position += vel.Velocity * delta;
		spr.Flash = 255;
		spr.Opacity = (byte)(dying.TimeLeftUntilDestroy / DyingMarkerComponent.Default.TimeLeftUntilDestroy * 255);
	}
}
