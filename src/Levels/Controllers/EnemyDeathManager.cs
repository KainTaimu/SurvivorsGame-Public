using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Arch.Buffer;
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

	public override void _Process(double delta)
	{
		var commands = new CommandBuffer();
		UpdateNewDeathsQuery(GameWorld.World, commands);
		UpdateDyingQuery(GameWorld.World, _pendingDeaths, (float)delta);

		while (_pendingDeaths.TryDequeue(out var entity))
		{
			commands.Destroy(entity);
		}

		if (commands.Size == 0)
			return;
		EntityCommandBuffer.Instance.PushCommand(commands);
	}

	[Query]
	[All<HealthComponent, PositionComponent>]
	[None<DyingMarkerComponent>]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void UpdateNewDeaths([Data] in CommandBuffer commandBuffer, Entity entity, ref HealthComponent health)
	{
		if (health.Health > 0)
			return;
		if (!GameWorld.World.IsAlive(entity))
			return;

		commandBuffer.Add(entity, DyingMarkerComponent.Default);

		if (GameWorld.World.TryGet<DeathRewardComponent>(entity, out var reward))
			LevelData.Instance?.Money += reward.Money;
		EmitSignalOnEnemyDeath(new EntityObject(entity));
	}

	[Query(Parallel = true)]
	[All<DyingMarkerComponent, PositionComponent, VelocityComponent, AnimatedSpriteComponent>]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void UpdateDying(
		[Data] in ConcurrentQueue<Entity> pendingDeaths,
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
			pendingDeaths.Enqueue(entity);
			return;
		}

		dying.TimeLeftUntilDestroy -= delta;
		pos.Position += vel.Velocity * delta;
		spr.Flash = 255;
		spr.Opacity = (byte)(dying.TimeLeftUntilDestroy / DyingMarkerComponent.Default.TimeLeftUntilDestroy * 255);
	}
}
