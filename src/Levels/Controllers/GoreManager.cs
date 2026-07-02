using System.Collections.Generic;
using Game.Core.ECS;
using Game.Core.Settings;

namespace Game.Levels.Controllers;

public partial class GoreManager : Node
{
	[Export]
	private PackedScene _enemyDeathParticlesScene = null!;

	[Export]
	private PackedScene _spurtParticlesScene = null!;

	[Export]
	private Godot.Collections.Dictionary<DeathCauseEnum, HitParticlesInfo> _deathParticleProcessMaterialsByCause = [];

	private int MaxActiveParticles
	{
		get => GameSettings.Instance.GoreEffectsValue;
		set => UpdateParticles(_activeParticles, _inactiveParticles, value, false);
	}

	private int MaxActiveSpurtParticles
	{
		get =>
			GameSettings.Instance.GoreEffects >= GoreEffectsEnum.Medium
				? Mathf.CeilToInt(MaxActiveParticles * 0.2f)
				: 0;
		set => UpdateParticles(_activeSpurtParticles, _inactiveSpurtParticles, value, true);
	}

	private readonly Queue<GpuParticles2D> _activeParticles = [];
	private readonly Queue<GpuParticles2D> _inactiveParticles = [];

	private readonly Queue<GpuParticles2D> _activeSpurtParticles = [];
	private readonly Queue<GpuParticles2D> _inactiveSpurtParticles = [];

	private ProcessModeEnum? _particlesOriginalProcessMode;

	public override void _Ready()
	{
		UpdateParticles(_activeParticles, _inactiveParticles, MaxActiveParticles, false);
		UpdateParticles(_activeSpurtParticles, _inactiveSpurtParticles, MaxActiveSpurtParticles, true);
		GameSettings.Instance.OnGoreEffectsChanged += () =>
		{
			UpdateParticles(_activeParticles, _inactiveParticles, MaxActiveParticles, false);
			UpdateParticles(_activeSpurtParticles, _inactiveSpurtParticles, MaxActiveSpurtParticles, true);
		};
	}

	// PERF: Large amount of particles causes a draw call per active particles.
	// TODO: Find out a way to reduce draw calls
	public void SpawnDeathParticles(Vector2 pos, DeathCauseEnum cause = DeathCauseEnum.Normal)
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
		EnableParticles(particles, pos, cause);

		_activeParticles.Enqueue(particles);
	}

	public void SpawnHitSpurtPaticles(Vector2 pos, float direction)
	{
		if (MaxActiveSpurtParticles <= 0)
			return;
		if (_activeSpurtParticles.Count >= MaxActiveSpurtParticles)
		{
			var off = _activeSpurtParticles.Dequeue();
			off.Hide();
			_inactiveSpurtParticles.Enqueue(off);
		}

		var particles = _inactiveSpurtParticles.Dequeue();
		particles.GlobalPosition = pos;
		particles.GlobalRotation = (float)(direction + Mathf.DegToRad(GD.RandRange(-9f, 9f)));
		particles.Show();
		particles.Restart();
		particles.ProcessMode = _particlesOriginalProcessMode ?? ProcessModeEnum.Inherit;

		_activeSpurtParticles.Enqueue(particles);
	}

	private void UpdateParticles(
		Queue<GpuParticles2D> activeQueue,
		Queue<GpuParticles2D> inactiveQueue,
		int maxParticles,
		bool isSpurtParticles
	)
	{
		var particlesLeft = maxParticles - activeQueue.Count - inactiveQueue.Count;

		while (particlesLeft < 0)
		{
			if (!inactiveQueue.TryDequeue(out var particles))
				break;
			particles.QueueFree();
			particlesLeft++;
		}

		while (particlesLeft < 0)
		{
			if (!activeQueue.TryDequeue(out var particles))
				break;
			particles.QueueFree();
			particlesLeft++;
		}

		for (var i = 0; i < particlesLeft; i++)
		{
			var particles = isSpurtParticles
				? _spurtParticlesScene.Instantiate<GpuParticles2D>()
				: _enemyDeathParticlesScene.Instantiate<GpuParticles2D>();
			particles.Name = isSpurtParticles ? $"Spurt_{i}" : $"Death_{i}";

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
			inactiveQueue.Enqueue(particles);
		}
	}

	private void EnableParticles(
		GpuParticles2D particles,
		Vector2 position,
		DeathCauseEnum cause = DeathCauseEnum.Normal
	)
	{
		particles.GlobalPosition = position;
		particles.GlobalRotation = 0;
		particles.Show();
		particles.Restart();
		particles.ProcessMode = _particlesOriginalProcessMode ?? ProcessModeEnum.Inherit;
		var info = _deathParticleProcessMaterialsByCause[cause];
		particles.ProcessMaterial = info.ProcessMaterial;
	}

	private void DisableParticles(GpuParticles2D particles)
	{
		particles.Hide();
		particles.ProcessMode = ProcessModeEnum.Disabled;
	}
}
