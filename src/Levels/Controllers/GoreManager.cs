using System.Collections.Generic;
using Game.Core.ECS;
using Game.Core.Settings;

namespace Game.Levels.Controllers;

public partial class GoreManager : Node
{
	[Export]
	private PackedScene _enemyDeathParticlesScene = null!;

	private EntityComponentStore ComponentStore =>
		EntityComponentStore.Instance;

	private int MaxActiveParticles
	{
		get => GameSettings.Instance.GoreEffectsValue;
		set { UpdateParticles(value); }
	}

	private readonly Queue<GpuParticles2D> _activeParticles = [];
	private readonly Queue<GpuParticles2D> _inactiveParticles = [];

	private ProcessModeEnum? _particlesOriginalProcessMode;

	public override void _Ready()
	{
		UpdateParticles(MaxActiveParticles);
		GameSettings.Instance.OnGoreEffectsChanged += () =>
			UpdateParticles(MaxActiveParticles);
	}

	// PERF: Large amount of particles causes a draw call per active particles.
	// TODO: Find out a way to reduce draw calls
	public void SpawnParticles(Vector2 pos)
	{
		if (MaxActiveParticles <= 0)
			return;
		if (_activeParticles.Count >= MaxActiveParticles)
		{
			var off = _activeParticles.Dequeue();
			off.Hide();
			_inactiveParticles.Enqueue(off);
		}

		var particles = _inactiveParticles.Dequeue();
		EnableParticles(particles, pos);

		_activeParticles.Enqueue(particles);
	}

	private void UpdateParticles(int maxParticles)
	{
		if (_enemyDeathParticlesScene is null)
			return;

		var particlesLeft =
			maxParticles - _activeParticles.Count - _inactiveParticles.Count;

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
		}
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
