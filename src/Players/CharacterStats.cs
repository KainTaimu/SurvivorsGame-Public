using Game.Models;

namespace Game.Players;

/// <summary>
/// Removing or adding fields requires changing CharacterStatType, and the switch inside
/// PlayerStatusEffectController.InitializeStatStacks
/// </summary>
[GlobalClass]
public partial class CharacterStats : Node
{
	[Export]
	private IntStat _health = null!;

	[Export]
	private IntStat _maxHealth = null!;

	[Export]
	private FloatStat _moveSpeed = null!;

	[Export]
	private FloatStat _runSpeed = null!;

	[Export]
	private FloatStat _stamina = null!;

	[Export]
	private FloatStat _maxStamina = null!;

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

	[Export]
	private FloatStat _criticalDamage = null!;

	[Export]
	private FloatStat _attackSpeed = null!;

	[Export]
	private FloatStat _bloomScale = null!;

	[Export]
	private FloatStat _recoilScale = null!;

	[Export]
	private FloatStat _incomingDamage = null!;

	[Export]
	private FloatStat _outgoingDamage = null!;

	public int Health => _health.Value;

	public int MaxHealth => _maxHealth.Value;

	public float MoveSpeed => _moveSpeed.Value;

	public float RunSpeed => _runSpeed.Value;

	public float Stamina
	{
		get => _stamina.Value;
		set => _stamina.BaseValue = Mathf.Clamp(value, 0, MaxStamina);
	}

	public float MaxStamina => _maxStamina.Value;

	public int Defense => _defense.Value;

	public float CriticalChance => _criticalChance.Value;

	public float PickupRangeRadius => _pickupRangeRadius.Value;

	public int HealthRegenPerSecond => _healthRegenPerSecond.Value;

	public float InvincibilityTime => _invincibilityTime.Value;

	public float HitboxRadius => _hitboxRadius.Value;

	public float CriticalDamage => _criticalDamage.Value;

	public float AttackSpeed => _attackSpeed.Value;

	public float BloomScale => _bloomScale.Value;

	public float RecoilScale => _recoilScale.Value;

	public float IncomingDamage => _incomingDamage.Value;

	public float OutgoingDamage => _outgoingDamage.Value;

	[ExportCategory("Internal")]
	[Export]
	private Player _player = null!;

	public void Damage(int damage)
	{
		var damageAfterDefense = damage - Defense;
		var scaledDamage = damageAfterDefense * IncomingDamage;
		var clampedDamage = Math.Clamp(scaledDamage, 1, float.PositiveInfinity);
		var totalDamage = Mathf.CeilToInt(clampedDamage);

		_health.BaseValue -= totalDamage;
		_player.EmitSignal(Player.SignalName.OnDamaged, totalDamage);
	}
}
