using Game.Core.ECS;
using Game.Core.Settings;
using Game.Items.Projectiles;

namespace Game.Levels.Controllers;

public partial class GoreManager : Node2D
{
	[Export]
	private EnemyDeathManager? _deathManager;

	[ExportGroup("Particles")]
	[Export]
	private GoreBurstParams _deathNormalParams = null!;

	[Export]
	private GoreBurstParams _deathExplosionParams = null!;

	[Export]
	private GoreBurstParams _spurtParams = null!;

	[ExportGroup("Internal")]
	[Export]
	private GoreParticleBuffer _particleBuffer = null!;

	private static int MaxParticleCount => GameSettings.Instance.GoreEffectsValue;

	public override void _Ready()
	{
		_particleBuffer.Initialize(MaxParticleCount, _deathNormalParams, _deathExplosionParams, _spurtParams);
		GameSettings.Instance.OnGoreEffectsChanged += OnGoreSettingsChanged;

		_deathManager?.OnEnemyDeath += OnEnemyDeath;
	}

	public override void _ExitTree()
	{
		GameSettings.Instance.OnGoreEffectsChanged -= OnGoreSettingsChanged;
	}

	public void SpawnDeathParticles(Vector2 pos, DeathCauseEnum cause = DeathCauseEnum.Normal)
	{
		_particleBuffer.SpawnDeathBurst(pos, cause);
	}

	public void SpawnHitSpurtPaticles(Vector2 pos, float direction)
	{
		_particleBuffer.SpawnSpurtBurst(pos, direction);
	}

	private void OnGoreSettingsChanged()
	{
		_particleBuffer.SetCapacity(MaxParticleCount);
	}

	private void OnEnemyDeath(EntityObject entity)
	{
		if (!GameWorld.World.Has<PositionComponent>(entity.Entity))
			return;
		var pos = GameWorld.World.Get<PositionComponent>(entity.Entity);
		if (GameWorld.World.TryGet<DeathCauseComponent>(entity.Entity, out var cause))
			SpawnDeathParticles(pos.Position, cause.CauseEnum);
		else
			SpawnDeathParticles(pos.Position);
	}
}
