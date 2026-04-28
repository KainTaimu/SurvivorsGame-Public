using Game.Core.ECS;

namespace Game.Levels.Controllers;

public partial class EnemyDeathManager : Node
{
	[Export]
	private PackedScene _enemyDeathParticlesScene = null!;

	[Export]
	private EntityComponentStore ComponentStore = null!;

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
		if (!ComponentStore.GetComponent<PositionComponent>(id, out var pos))
		{
			ComponentStore.UnregisterEntity(id);
			return;
		}
		ComponentStore.UnregisterEntity(id);

		var particleNode = _enemyDeathParticlesScene.Instantiate();
		switch (particleNode)
		{
			case CpuParticles2D cpu:
				cpu.GlobalPosition = pos.Position;
				cpu.Emitting = true;
				break;
			case GpuParticles2D gpu:
				gpu.GlobalPosition = pos.Position;
				gpu.Emitting = true;
				break;
			default:
				particleNode.Free();
				return;
		}

		AddChild(particleNode);
	}
}
