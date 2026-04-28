using System.Collections.Generic;
using Game.Core.ECS;

namespace Game.Levels.Controllers;

public partial class EnemyDeathManager : Node
{
	[Export]
	private PackedScene _enemyDeathParticlesScene = null!;

	[Export]
	private EntityComponentStore ComponentStore = null!;

	[Export]
	private int _maxActiveParticles = 100;

	private readonly Queue<GpuParticles2D> _activeParticles = [];
	private readonly Queue<GpuParticles2D> _inactiveParticles = [];

	public override void _Ready()
	{
		if (_enemyDeathParticlesScene is null)
			return;

		for (var i = 0; i < _maxActiveParticles; i++)
		{
			var gpu = _enemyDeathParticlesScene.Instantiate<GpuParticles2D>();
			gpu.Hide();
			gpu.Finished += gpu.Hide;
			AddChild(gpu);
			_inactiveParticles.Enqueue(gpu);
		}
	}

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

		if (_activeParticles.Count >= _maxActiveParticles)
		{
			var off = _activeParticles.Dequeue();
			off.Hide();
			_inactiveParticles.Enqueue(off);
		}

		var gpu = _inactiveParticles.Dequeue();
		gpu.GlobalPosition = pos.Position;
		gpu.Show();
		gpu.Restart();
		_activeParticles.Enqueue(gpu);
	}
}
