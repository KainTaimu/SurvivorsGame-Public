using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using Game.Core.ECS;
using Game.Items.Projectiles;

namespace Game.Levels.Controllers;

public partial class EnemyDeathManager : Node
{
	[Signal]
	public delegate void OnEnemyDeathEventHandler(EntityObject entity);

	private readonly Queue<Entity> _dyingToRemove = [];

	public override void _Process(double delta)
	{
		UpdateNewDeathsQuery(GameWorld.World);
		UpdateDyingQuery(GameWorld.World, (float)delta);

		CallDeferred(MethodName.DestroyDeadEnemies);
	}

	private void DestroyDeadEnemies()
	{
		while (_dyingToRemove.TryDequeue(out var entity))
		{
			if (GameWorld.World.IsAlive(entity))
				GameWorld.World.Destroy(entity);
		}
	}

	[Query]
	[All<HealthComponent, PositionComponent>]
	[None<DyingMarkerComponent>]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void UpdateNewDeaths(in Entity entity, ref HealthComponent health)
	{
		if (health.Health <= 0)
			HandleDeath(entity);
	}

	[Query]
	[All<DyingMarkerComponent, PositionComponent, VelocityComponent, AnimatedSpriteComponent>]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void UpdateDying(
		[Data] in float delta,
		in Entity entity,
		ref DyingMarkerComponent dying,
		ref PositionComponent pos,
		ref VelocityComponent vel,
		ref AnimatedSpriteComponent spr
	)
	{
		if (dying.TimeLeftUntilDestroy <= 0)
		{
			_dyingToRemove.Enqueue(entity);
			return;
		}

		dying.TimeLeftUntilDestroy -= delta;
		pos.Position += vel.Velocity * delta;
		spr.Flash = 255;
		spr.Opacity = (byte)(dying.TimeLeftUntilDestroy / DyingMarkerComponent.Default.TimeLeftUntilDestroy * 255);
	}

	private void HandleDeath(Entity entity)
	{
		if (!GameWorld.World.IsAlive(entity))
			return;
		GameWorld.World.Add(entity, DyingMarkerComponent.Default);

		if (GameWorld.World.TryGet<DeathRewardComponent>(entity, out var reward))
			LevelData.Instance?.Money += reward.Money;
		EmitSignalOnEnemyDeath(new EntityObject(entity));
	}
}
