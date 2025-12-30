namespace Game.Players;

// =============
// Each Stat contains a XChangedEventHandler which fires when one is changed.
// Most Stat setters are the same, except for Health which is clamped between 0 to MaxHealth, and MaxHealth which clamps Health to its value.
// Health is initially set to MaxHealth.
// =============
public partial class CharacterStats : Node
{
	[Signal]
	public delegate void OnHealthChangedEventHandler(int oldValue, int newValue);

	[Signal]
	public delegate void OnMaxHealthChangedEventHandler(int oldValue, int newValue);

	[Signal]
	public delegate void OnMoveSpeedChangedEventHandler(float oldValue, float newValue);

	[Signal]
	public delegate void OnDefenseChangedEventHandler(int oldValue, int newValue);

	[Signal]
	public delegate void OnCriticalChanceChangedEventHandler(float oldValue, float newValue);

	[Signal]
	public delegate void OnPickupRangeRadiusChangedEventHandler(float oldValue, float newValue);

	[Signal]
	public delegate void OnHealthRegenPerSecondChangedEventHandler(int oldValue, int newValue);

	[Signal]
	public delegate void OnInvincibilityTimeChangedEventHandler(float oldValue, float newValue);

	[Signal]
	public delegate void OnHealthMultiplierChangedEventHandler(float oldValue, float newValue);

	[Signal]
	public delegate void OnMoveSpeedMultiplierChangedEventHandler(float oldValue, float newValue);

	[Signal]
	public delegate void OnIncomingDamageMultiplierChangedEventHandler(
		float oldValue,
		float newValue
	);

	[Signal]
	public delegate void OnOutgoingDamageMultiplierChangedEventHandler(
		float oldValue,
		float newValue
	);

	[Signal]
	public delegate void OnCriticalChanceMultiplierChangedEventHandler(
		float oldValue,
		float newValue
	);

	[Signal]
	public delegate void OnCriticalDamageMultiplierChangedEventHandler(
		float oldValue,
		float newValue
	);

	[Signal]
	public delegate void OnAttackSpeedMultiplierChangedEventHandler(float oldValue, float newValue);

	[Signal]
	public delegate void OnProjectileMultiplierChangedEventHandler(float oldValue, float newValue);

	[Signal]
	public delegate void OnXpMultiplierChangedEventHandler(float oldValue, float newValue);

	private int _health;
	private int _maxHealth = 100;
	private float _moveSpeed = 800;
	private int _defense;
	private float _criticalChance;
	private float _pickupRangeRadius = 500;
	private int _healthRegenPerSecond;
	private float _invincibilityTime;

	private float _healthMultiplier = 1;
	private float _moveSpeedMultiplier = 1;
	private float _incomingDamageMultiplier = 1;
	private float _outgoingDamageMultiplier = 1;
	private float _criticalChanceMultiplier = 1;
	private float _criticalDamageMultiplier = 1;
	private float _attackSpeedMultiplier = 1;
	private float _projectileMultiplier = 1;
	private float _xpMultiplier = 1;

	public int Health
	{
		get => _health;
		set
		{
			var clamped = Math.Clamp(value, 0, _maxHealth);
			if (_health == clamped)
				return;

			var oldValue = _health;
			_health = clamped;
			EmitSignal(SignalName.OnHealthChanged, oldValue, _health);
		}
	}

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public required int MaxHealth
	{
		get => _maxHealth;
		set
		{
			var clamped = Math.Clamp(value, 0, int.MaxValue);
			if (_maxHealth == clamped)
				return;

			var oldValue = _maxHealth;
			_maxHealth = clamped;
			EmitSignal(SignalName.OnMaxHealthChanged, oldValue, _maxHealth);

			if (_health > _maxHealth)
				Health = _maxHealth;
		}
	}

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public required float MoveSpeed
	{
		get => _moveSpeed;
		set
		{
			var clamped = Math.Max(value, 0);
			if (_moveSpeed == clamped)
				return;

			var oldValue = _moveSpeed;
			_moveSpeed = clamped;
			EmitSignal(SignalName.OnMoveSpeedChanged, oldValue, _moveSpeed);
		}
	}

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public required int Defense
	{
		get => _defense;
		set
		{
			var clamped = Math.Clamp(value, 0, int.MaxValue);
			if (_defense == clamped)
				return;

			var oldValue = _defense;
			_defense = clamped;
			EmitSignal(SignalName.OnDefenseChanged, oldValue, _defense);
		}
	}

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public required float CriticalChance
	{
		get => _criticalChance;
		set
		{
			var clamped = Math.Max(value, 0);
			if (_criticalChance == clamped)
				return;

			var oldValue = _criticalChance;
			_criticalChance = clamped;
			EmitSignal(SignalName.OnCriticalChanceChanged, oldValue, _criticalChance);
		}
	}

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public required float PickupRangeRadius
	{
		get => _pickupRangeRadius;
		set
		{
			var clamped = Math.Max(value, 0);
			if (_pickupRangeRadius == clamped)
				return;

			var oldValue = _pickupRangeRadius;
			_pickupRangeRadius = clamped;
			EmitSignal(SignalName.OnPickupRangeRadiusChanged, oldValue, _pickupRangeRadius);
		}
	}

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public required int HealthRegenPerSecond
	{
		get => _healthRegenPerSecond;
		set
		{
			var clamped = Math.Clamp(value, 0, int.MaxValue);
			if (_healthRegenPerSecond == clamped)
				return;

			var oldValue = _healthRegenPerSecond;
			_healthRegenPerSecond = clamped;
			EmitSignal(SignalName.OnHealthRegenPerSecondChanged, oldValue, _healthRegenPerSecond);
		}
	}

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public required float InvincibilityTime
	{
		get => _invincibilityTime;
		set
		{
			var clamped = Math.Max(value, 0);
			if (_invincibilityTime == clamped)
				return;

			var oldValue = _invincibilityTime;
			_invincibilityTime = clamped;
			EmitSignal(SignalName.OnInvincibilityTimeChanged, oldValue, _invincibilityTime);
		}
	}

	[ExportCategory("Multiplier attributes")]
	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public required float HealthMultiplier
	{
		get => _healthMultiplier;
		set
		{
			var clamped = Math.Max(value, 0);
			if (_healthMultiplier == clamped)
				return;

			var oldValue = _healthMultiplier;
			_healthMultiplier = clamped;
			EmitSignal(SignalName.OnHealthMultiplierChanged, oldValue, _healthMultiplier);
		}
	}

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public required float MoveSpeedMultiplier
	{
		get => _moveSpeedMultiplier;
		set
		{
			var clamped = Math.Max(value, 0);
			if (_moveSpeedMultiplier == clamped)
				return;

			var oldValue = _moveSpeedMultiplier;
			_moveSpeedMultiplier = clamped;
			EmitSignal(SignalName.OnMoveSpeedMultiplierChanged, oldValue, _moveSpeedMultiplier);
		}
	}

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public required float IncomingDamageMultiplier
	{
		get => _incomingDamageMultiplier;
		set
		{
			var clamped = Math.Max(value, 0);
			if (_incomingDamageMultiplier == clamped)
				return;

			var oldValue = _incomingDamageMultiplier;
			_incomingDamageMultiplier = clamped;
			EmitSignal(
				SignalName.OnIncomingDamageMultiplierChanged,
				oldValue,
				_incomingDamageMultiplier
			);
		}
	}

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public required float OutgoingDamageMultiplier
	{
		get => _outgoingDamageMultiplier;
		set
		{
			var clamped = Math.Max(value, 0);
			if (_outgoingDamageMultiplier == clamped)
				return;

			var oldValue = _outgoingDamageMultiplier;
			_outgoingDamageMultiplier = clamped;
			EmitSignal(
				SignalName.OnOutgoingDamageMultiplierChanged,
				oldValue,
				_outgoingDamageMultiplier
			);
		}
	}

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public required float CriticalChanceMultiplier
	{
		get => _criticalChanceMultiplier;
		set
		{
			var clamped = Math.Max(value, 0);
			if (_criticalChanceMultiplier == clamped)
				return;

			var oldValue = _criticalChanceMultiplier;
			_criticalChanceMultiplier = clamped;
			EmitSignal(
				SignalName.OnCriticalChanceMultiplierChanged,
				oldValue,
				_criticalChanceMultiplier
			);
		}
	}

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public required float CriticalDamageMultiplier
	{
		get => _criticalDamageMultiplier;
		set
		{
			var clamped = Math.Max(value, 0);
			if (_criticalDamageMultiplier == clamped)
				return;

			var oldValue = _criticalDamageMultiplier;
			_criticalDamageMultiplier = clamped;
			EmitSignal(
				SignalName.OnCriticalDamageMultiplierChanged,
				oldValue,
				_criticalDamageMultiplier
			);
		}
	}

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public required float AttackSpeedMultiplier
	{
		get => _attackSpeedMultiplier;
		set
		{
			var clamped = Math.Max(value, 0);
			if (_attackSpeedMultiplier == clamped)
				return;

			var oldValue = _attackSpeedMultiplier;
			_attackSpeedMultiplier = clamped;
			EmitSignal(SignalName.OnAttackSpeedMultiplierChanged, oldValue, _attackSpeedMultiplier);
		}
	}

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public required float ProjectileMultiplier
	{
		get => _projectileMultiplier;
		set
		{
			var clamped = Math.Max(value, 0);
			if (_projectileMultiplier == clamped)
				return;

			var oldValue = _projectileMultiplier;
			_projectileMultiplier = clamped;
			EmitSignal(SignalName.OnProjectileMultiplierChanged, oldValue, _projectileMultiplier);
		}
	}

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public required float XpMultiplier
	{
		get => _xpMultiplier;
		set
		{
			var clamped = Math.Max(value, 0);
			if (_xpMultiplier == clamped)
				return;

			var oldValue = _xpMultiplier;
			_xpMultiplier = clamped;
			EmitSignal(SignalName.OnXpMultiplierChanged, oldValue, _xpMultiplier);
		}
	}

	public override void _Ready()
	{
		Health = MaxHealth;
	}
}
