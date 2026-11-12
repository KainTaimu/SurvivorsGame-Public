using System.Collections.Generic;
using Game.Core.ECS;
using Game.Core.Settings;
using Game.Items.Projectiles;

namespace Game.Levels.Controllers;

public partial class GoreManager : Node2D
{
	[Export]
	private EnemyDeathManager? _deathManager;

	[ExportCategory("Particles")]
	[Export]
	private GoreBurstParams _deathNormalParams = null!;

	[Export]
	private GoreBurstParams _deathExplosionParams = null!;

	[Export]
	private GoreBurstParams _spurtParams = null!;

	// using multiple particle buffers because performance slows once the max particle limit is hit and the buffer
	// begins reusing older indexes. i think its because updating the particle transforms and shader information
	// becomes IO bound due to the size of the particle buffers being uploaded to the GPU. but idk how godot works
	// with this stuff internally, just a guess. implementing this did improve performance though but did not
	// eliminate it completely
	private readonly List<GoreParticleBuffer> _particleBuffers = [];

	private int _nextBufferIndex;
	private static int MaxParticleCount => GameSettings.Instance.GoreEffectsValue;

	public override void _Ready()
	{
		foreach (var child in GetChildren())
		{
			if (child is not GoreParticleBuffer buffer)
				continue;
			_particleBuffers.Add(buffer);
		}

		foreach (var buffer in _particleBuffers)
		{
			buffer.Initialize(
				MaxParticleCount / _particleBuffers.Count,
				_deathNormalParams,
				_deathExplosionParams,
				_spurtParams
			);
		}

		GameSettings.Instance.OnGoreEffectsChanged += OnGoreSettingsChanged;

		_deathManager?.OnEnemyDeath += OnEnemyDeath;
	}

	public override void _ExitTree()
	{
		GameSettings.Instance.OnGoreEffectsChanged -= OnGoreSettingsChanged;
	}

	public void SpawnDeathParticles(Vector2 pos, DeathCauseEnum cause = DeathCauseEnum.Normal)
	{
		_nextBufferIndex = (_nextBufferIndex + 1) % _particleBuffers.Count;
		_particleBuffers[_nextBufferIndex].SpawnDeathBurst(pos, cause);
	}

	public void SpawnHitSpurtPaticles(Vector2 pos, float direction)
	{
		_nextBufferIndex = (_nextBufferIndex + 1) % _particleBuffers.Count;
		_particleBuffers[_nextBufferIndex].SpawnSpurtBurst(pos, direction);
	}

	private void OnGoreSettingsChanged()
	{
		foreach (var buffer in _particleBuffers)
			buffer.SetCapacity(MaxParticleCount);
	}

	private void OnEnemyDeath(EntityObject entity)
	{
		if (!GameWorld.World.IsAlive(entity.Entity))
			return;

		ref var pos = ref GameWorld.World.Get<PositionComponent>(entity.Entity);
		if (GameWorld.World.TryGet<DeathCauseComponent>(entity.Entity, out var cause))
			SpawnDeathParticles(pos.Position, cause.CauseEnum);
		else
			SpawnDeathParticles(pos.Position);
	}
}
