namespace Game.Players;

[GlobalClass]
public partial class CharacterStats : Resource
{
	private int _health = -1;

	[Export]
	private IntStat _maxHealth = null!;

	[Export]
	private FloatStat _moveSpeed = null!;

	[Export]
	private IntStat _defense = null!;

	[Export]
	private FloatStat _criticalChance = null!;

	[Export]
	private FloatStat _pickupRangeRadius = null!;

	[Export]
	private IntStat _healthRegenPerSecond = null!;

	[Export]
	private FloatStat _invincibilityTime = null!;

	[Export]
	private FloatStat _hitboxRadius = null!;

	[ExportCategory("Multiplier attributes")]
	[Export]
	private FloatStat _incomingDamageMultiplier = null!;

	[Export]
	private FloatStat _outgoingDamageMultiplier = null!;

	[Export]
	private FloatStat _criticalChanceMultiplier = null!;

	[Export]
	private FloatStat _criticalDamageMultiplier = null!;

	[Export]
	private FloatStat _attackSpeedMultiplier = null!;

	[Export]
	private FloatStat _projectileMultiplier = null!;

	[Export]
	private FloatStat _xpMultiplier = null!;

	public int Health
	{
		get
		{
			// health may be uninitialized on first access.
			if (_health == -1)
				_health = _maxHealth.Value;

			return _health;
		}
	}

	public int MaxHealth => _maxHealth.Value;

	public float MoveSpeed => _moveSpeed.Value;

	public int Defense => _defense.Value;

	public float PickupRangeRadius => _pickupRangeRadius.Value;

	public int HealthRegenPerSecond => _healthRegenPerSecond.Value;

	public float InvincibilityTime => _invincibilityTime.Value;

	public float HitboxRadius => _hitboxRadius.Value;

	public float IncomingDamageMultiplier => _incomingDamageMultiplier.Value;

	public float OutgoingDamageMultiplier => _outgoingDamageMultiplier.Value;

	public float CriticalChanceMultiplier => _criticalChanceMultiplier.Value;

	public float CriticalDamageMultiplier => _criticalDamageMultiplier.Value;

	public float AttackSpeedMultiplier => _attackSpeedMultiplier.Value;

	public float ProjectileMultiplier => _projectileMultiplier.Value;

	public float XpMultiplier => _xpMultiplier.Value;

	public void Damage(int damage)
	{
		var damageAfterDefense = damage - Defense;
		var scaledDamage = damageAfterDefense * IncomingDamageMultiplier;
		var clampedDamage = Math.Clamp(scaledDamage, 1, float.PositiveInfinity);
		var totalDamage = Mathf.CeilToInt(clampedDamage);

		Logger.LogDebug($"Player damaged {totalDamage}. Health: {Health - totalDamage}");
		_health -= totalDamage;
	}
}
