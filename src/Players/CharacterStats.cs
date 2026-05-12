namespace Game.Players;

[GlobalClass]
public partial class CharacterStats : Resource
{
	private int _health = -1;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	private int _maxHealth = 100;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	private float _moveSpeed = 600;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	private int _defense;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	private float _criticalChance;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	private float _pickupRangeRadius = 500;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	private int _healthRegenPerSecond;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	private float _invincibilityTime = 1;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	private float _hitboxRadius = 32;

	[ExportCategory("Multiplier attributes")]
	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	private float _healthMultiplier = 1;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	private float _moveSpeedMultiplier = 1;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	private float _defenseMultiplier = 1;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	private float _incomingDamageMultiplier = 1;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	private float _outgoingDamageMultiplier = 1;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	private float _criticalChanceMultiplier = 1;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	private float _criticalDamageMultiplier = 1;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	private float _attackSpeedMultiplier = 1;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	private float _projectileMultiplier = 1;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	private float _xpMultiplier = 1;

	public int Health
	{
		get
		{
			// health may be uninitialized on first access.
			if (_health == -1)
				_health = _maxHealth;

			return _health;
		}
	}

	public int MaxHealth
	{
		get { return Mathf.CeilToInt(_maxHealth * _healthMultiplier); }
	}

	public float MoveSpeed
	{
		get { return _moveSpeed * _moveSpeedMultiplier; }
	}

	// Flat defense
	public int Defense
	{
		get { return Mathf.CeilToInt(_defense * _defenseMultiplier); }
	}

	public float CriticalChance
	{
		get { return _criticalChance * _criticalChanceMultiplier; }
	}

	public float PickupRangeRadius
	{
		get { return _pickupRangeRadius; }
	}

	public int HealthRegenPerSecond
	{
		get { return _healthRegenPerSecond; }
	}

	public float InvincibilityTime
	{
		get { return _invincibilityTime; }
	}

	public float HitboxRadius
	{
		get { return _hitboxRadius; }
	}

	public float HealthMultiplier
	{
		get { return _healthMultiplier; }
	}

	public float MoveSpeedMultiplier
	{
		get { return _moveSpeedMultiplier; }
	}

	public float DefenseMultiplier
	{
		get { return _defenseMultiplier; }
	}

	public float IncomingDamageMultiplier
	{
		get { return _incomingDamageMultiplier; }
	}

	public float OutgoingDamageMultiplier
	{
		get { return _outgoingDamageMultiplier; }
	}

	public float CriticalChanceMultiplier
	{
		get { return _criticalChanceMultiplier; }
	}

	public float CriticalDamageMultiplier
	{
		get { return _criticalDamageMultiplier; }
	}

	public float AttackSpeedMultiplier
	{
		get { return _attackSpeedMultiplier; }
	}

	public float ProjectileMultiplier
	{
		get { return _projectileMultiplier; }
	}

	public float XpMultiplier
	{
		get { return _xpMultiplier; }
	}

	public int TotalMaxHealth
	{
		get { return Mathf.CeilToInt(MaxHealth * HealthMultiplier); }
	}

	public float TotalMoveSpeed
	{
		get { return MoveSpeed * MoveSpeedMultiplier; }
	}

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
