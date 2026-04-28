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
	private int MaxActiveParticles
	{
		get;
		set
		{
			UpdateParticles(value);
			field = value;
		}
	} = 100;

	private readonly Queue<GpuParticles2D> _activeParticles = [];
	private readonly Queue<GpuParticles2D> _inactiveParticles = [];

	private ProcessModeEnum? _particlesOriginalProcessMode;

	private void UpdateParticles(int maxParticles)
	{
		if (_enemyDeathParticlesScene is null)
			return;

		var particlesLeft =
			maxParticles - _activeParticles.Count - _inactiveParticles.Count;

		Logger.LogDebug($"start: {particlesLeft}");
		while (particlesLeft < 0)
		{
			if (!_inactiveParticles.TryDequeue(out var particles))
				break;
			particles.QueueFree();
			particlesLeft++;
		}
		while (particlesLeft < 0)
		{
			if (!_activeParticles.TryDequeue(out var particles))
				break;
			particles.QueueFree();
			particlesLeft++;
		}

		for (var i = 0; i < particlesLeft; i++)
		{
			var particles =
				_enemyDeathParticlesScene.Instantiate<GpuParticles2D>();
			_particlesOriginalProcessMode ??= particles.ProcessMode;

			particles.Finished += () =>
			{
				if (!IsInstanceValid(particles))
					return;
				particles.Hide();
				DisableParticles(particles);
			};
			DisableParticles(particles);
			AddChild(particles);
			_inactiveParticles.Enqueue(particles);
			Logger.LogDebug(
				$"inactive({_inactiveParticles.Count}): {particlesLeft}"
			);
		}
	}

	public override void _Ready()
	{
		UpdateParticles(MaxActiveParticles);
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

		if (MaxActiveParticles <= 0)
			return;
		if (_activeParticles.Count >= MaxActiveParticles)
		{
			var off = _activeParticles.Dequeue();
			off.Hide();
			_inactiveParticles.Enqueue(off);
		}

		var particles = _inactiveParticles.Dequeue();
		EnableParticles(particles, pos.Position);

		_activeParticles.Enqueue(particles);
	}

	private void EnableParticles(GpuParticles2D particles, Vector2 position)
	{
		particles.GlobalPosition = position;
		particles.Show();
		particles.Restart();
		particles.ProcessMode =
			_particlesOriginalProcessMode ?? ProcessModeEnum.Inherit;
	}

	private void DisableParticles(GpuParticles2D particles)
	{
		particles.Hide();
		particles.ProcessMode = ProcessModeEnum.Disabled;
	}
}
