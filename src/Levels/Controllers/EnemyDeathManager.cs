using Game.Core.ECS;

namespace Game.Levels.Controllers;

public partial class EnemyDeathManager : Node
{
	[Export]
	public GoreManager? GoreManager;

	private EntityComponentStore ComponentStore =>
		EntityComponentStore.Instance;

	public override void _Process(double delta)
	{
		foreach (var (id, health) in ComponentStore.Query<HealthComponent>())
		{
			if (health.Health <= 0)
				HandleDeath(id);
		}
	}

	private void HandleDeath(int id)
	{
		if (ComponentStore.GetComponent<PositionComponent>(id, out var pos))
			GoreManager?.SpawnParticles(pos.Position);

		if (
			ComponentStore.GetComponent<DeathRewardComponent>(
				id,
				out var reward
			)
		)
			LevelData.Instance?.Money += reward.Money;

		ComponentStore.UnregisterEntity(id);
	}
}
