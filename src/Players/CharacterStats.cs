using Game.Models;

namespace Game.Players;

/// <summary>
/// Removing or adding fields requires changing CharacterStatType, and the switch inside
/// PlayerStatusEffectController.InitializeStatStacks
/// </summary>
[Tool]
public partial class CharacterStats : Node
{
	[Export]
	private IntStat _health = null!;

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
	private FloatStat _moveSpeedMultiplier = null!;

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
	private FloatStat _bloomMultiplier = null!;

	[Export]
	private FloatStat _recoilMultiplier = null!;

	[Export]
	private FloatStat _xpMultiplier = null!;

	[ExportGroup("Internal")]
	[Export]
	private bool PopulateStats
	{
		get;
		set
		{
			if (!value)
				return;
			field = value;
			field = false;
			_health = new() { BaseValue = 100 };
			_maxHealth = new() { BaseValue = 100 };
			_moveSpeed = new() { BaseValue = 600 };
			_defense = new();
			_criticalChance = new();
			_pickupRangeRadius = new();
			_healthRegenPerSecond = new();
			_invincibilityTime = new() { BaseValue = 0.5f };
			_hitboxRadius = new() { BaseValue = 42f };
			_moveSpeedMultiplier = new();
			_incomingDamageMultiplier = new();
			_outgoingDamageMultiplier = new();
			_criticalChanceMultiplier = new();
			_criticalDamageMultiplier = new();
			_attackSpeedMultiplier = new();
			_bloomMultiplier = new();
			_recoilMultiplier = new();
			_xpMultiplier = new();
		}
	}

	public int Health => _health.Value;

	public int MaxHealth => _maxHealth.Value;

	public float MoveSpeed => _moveSpeed.Value;

	public int Defense => _defense.Value;

	public float PickupRangeRadius => _pickupRangeRadius.Value;

	public int HealthRegenPerSecond => _healthRegenPerSecond.Value;

	public float InvincibilityTime => _invincibilityTime.Value;

	public float HitboxRadius => _hitboxRadius.Value;

	public float MoveSpeedMultiplier => _moveSpeedMultiplier.Value;

	public float IncomingDamageMultiplier => _incomingDamageMultiplier.Value;

	public float OutgoingDamageMultiplier => _outgoingDamageMultiplier.Value;

	public float CriticalChanceMultiplier => _criticalChanceMultiplier.Value;

	public float CriticalDamageMultiplier => _criticalDamageMultiplier.Value;

	public float AttackSpeedMultiplier => _attackSpeedMultiplier.Value;

	public float BloomMultiplier => _bloomMultiplier.Value;

	public float RecoilMultiplier => _recoilMultiplier.Value;

	public float XpMultiplier => _xpMultiplier.Value;

	public void Damage(int damage)
	{
		var damageAfterDefense = damage - Defense;
		var scaledDamage = damageAfterDefense * IncomingDamageMultiplier;
		var clampedDamage = Math.Clamp(scaledDamage, 1, float.PositiveInfinity);
		var totalDamage = Mathf.CeilToInt(clampedDamage);

		_health.BaseValue -= totalDamage;
	}
}
