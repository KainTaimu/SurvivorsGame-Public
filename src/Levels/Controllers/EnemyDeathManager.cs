using Game.Core.ECS;

namespace Game.Levels.Controllers;

public partial class EnemyDeathManager : Node
{
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
			return;
		ComponentStore.GetComponent<AnimatedSpriteComponent>(id, out var spr);
		ComponentStore.SetComponent(id, spr with { Flash = 255 });
		ComponentStore.UnregisterEntity(id);
		var particles = GD.Load<PackedScene>("uid://rv526axhe2xw")
			.Instantiate<CpuParticles2D>();
		AddChild(particles);
		particles.GlobalPosition = pos.Position;
		particles.Emitting = true;
	}
}
