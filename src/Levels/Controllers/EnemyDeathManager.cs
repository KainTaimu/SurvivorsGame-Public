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
			in new QueryDescription().WithAll<HealthComponent, PositionComponent>().WithNone<DyingMarkerComponent>(),
			(entity, ref health, ref position) =>
			{
				if (health.Health <= 0)
					HandleDeath(entity, ref position);
			}
		);

		GameWorld.World.Query<DyingMarkerComponent, PositionComponent, VelocityComponent, AnimatedSpriteComponent>(
			in new QueryDescription().WithAll<
				DyingMarkerComponent,
				PositionComponent,
				VelocityComponent,
				AnimatedSpriteComponent
			>(),
			(entity, ref dying, ref pos, ref vel, ref spr) =>
			{
				if (dying.TimeLeftUntilDestroy <= 0)
				{
					Callable
						.From(() =>
						{
							GameWorld.World.Destroy(entity);
						})
						.CallDeferred();
					return;
				}

				dying.TimeLeftUntilDestroy -= (float)delta;
				pos.Position += vel.Velocity * (float)delta;
				spr.Flash = 255;
				spr.Opacity = (byte)(
					dying.TimeLeftUntilDestroy / DyingMarkerComponent.Default.TimeLeftUntilDestroy * 255
				);
			}
		);
	}

	private void HandleDeath(Entity entity, ref PositionComponent pos)
	{
		var deathPos = pos.Position;

		GameWorld.World.Add(entity, DyingMarkerComponent.Default);

		if (GameWorld.World.TryGet<DeathCauseComponent>(entity, out var cause))
			GoreManager?.SpawnDeathParticles(deathPos, cause.CauseEnum);
		else
			GoreManager?.SpawnDeathParticles(deathPos);

		if (GameWorld.World.TryGet<DeathRewardComponent>(entity, out var reward))
			LevelData.Instance?.Money += reward.Money;
	}
}
