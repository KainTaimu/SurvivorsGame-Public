namespace Game.Items.Offensive;

[Tool]
[GlobalClass]
public partial class BurstFireGroup : AbstractFireGroup, ICooldown
{
	public float TimeBetweenBursts { get; set; }
	public float CooldownDuration { get; set; }

	private bool _isFireQueued;
	private float _cooldown;
	private int _shotsRemaining;

	[Export]
	private int _burstCount = 3;

	// Allow queuing a shot if _cooldown is FIRE_QUEUE_TOLERANCE% of TimeBetweenBursts
	[Export]
	private float _fireQueueTolerance = 0.5f;

	public override void ProcessInput()
	{
		if (_shotsRemaining > 0)
			return;

		if (!Input.IsActionJustPressed(InputMapNames.PrimaryAttack))
			return;

		if (_cooldown <= 0)
		{
			StartBurst();
			return;
		}

		var inQueueWindow = _cooldown <= TimeBetweenBursts * _fireQueueTolerance;
		if (inQueueWindow && !_isFireQueued)
			_isFireQueued = true;
	}

	private void StartBurst()
	{
		_shotsRemaining = _burstCount;
		_isFireQueued = false;
		_cooldown = 0;
	}

	public void Process(float delta)
	{
		if (_shotsRemaining > 0)
		{
			_cooldown = Mathf.Clamp(_cooldown - delta, 0, CooldownDuration);
			if (_cooldown > 0)
				return;

			_shotsRemaining--;
			_cooldown = _shotsRemaining > 0 ? CooldownDuration : TimeBetweenBursts;
			EmitSignalOnFire();
			return;
		}

		_cooldown = Mathf.Clamp(_cooldown - delta, 0, TimeBetweenBursts);
		if (_cooldown <= 0 && _isFireQueued)
			StartBurst();
	}
}
