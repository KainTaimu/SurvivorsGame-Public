using Arch.Core;
using Game.Core.ECS;

namespace Game.Levels.Controllers;

public partial class EnemyDeathManager : Node
{
	[Export]
	public GoreManager? GoreManager;

	public override void _Process(double delta)
	{
		GameWorld.World.Query<HealthComponent, PositionComponent>(
			in new QueryDescription().WithAll<HealthComponent>(),
			(entity, ref health, ref position) =>
			{
				// if (Engine.GetProcessFrames() % 10 == 0)
				// 	Logger.LogDebug($"id={entity.Id} version={entity.Version} health={health.Health}");
				if (health.Health <= 0)
					HandleDeath(entity, ref position);
			}
		);
	}

	private void HandleDeath(Entity entity, ref PositionComponent pos)
	{
		if (GameWorld.World.TryGet<DeathCauseComponent>(entity, out var cause))
			GoreManager?.SpawnDeathParticles(pos.Position, cause.CauseEnum);
		else
			GoreManager?.SpawnDeathParticles(pos.Position);

		if (GameWorld.World.TryGet<DeathRewardComponent>(entity, out var reward))
			LevelData.Instance?.Money += reward.Money;

		Callable
			.From(() =>
			{
				DestroyEntity(entity);
			})
			.CallDeferred();
	}

	private static void DestroyEntity(Entity entity)
	{
		GameWorld.World.Destroy(entity);
	}
}
