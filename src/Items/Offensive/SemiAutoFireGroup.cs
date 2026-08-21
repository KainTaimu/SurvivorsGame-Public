namespace Game.Items.Offensive;

[Tool]
[GlobalClass]
public partial class SemiAutoFireGroup : AbstractFireGroup, ICooldown
{
	public float CooldownDuration { get; set; }

	private bool _isFireQueued;
	private ulong _ticksSinceLastFire;
	private float _cooldown;

	// Allow queuing a shot if _cooldown is FIRE_QUEUE_TOLERANCE% of CooldownDuration
	[Export]
	private float _fireQueueTolerance = 0.5f;

	public override void ProcessInput()
	{
		if (!Input.IsActionJustPressed(InputMapNames.PrimaryAttack))
			return;

		if (_cooldown <= 0)
		{
			ResetOnFire();
			EmitSignalOnFire();
			return;
		}

		var inQueueWindow = _cooldown <= CooldownDuration * _fireQueueTolerance;
		if (inQueueWindow && !_isFireQueued)
			_isFireQueued = true;
	}

	private void ResetOnFire()
	{
		_cooldown = CooldownDuration;
		_isFireQueued = false;
		_ticksSinceLastFire = Time.GetTicksMsec();
	}

	public void Process(float delta)
	{
		_cooldown = Mathf.Clamp(_cooldown - delta, 0, CooldownDuration);
		if (_cooldown <= 0 && _isFireQueued)
		{
			ResetOnFire();
			EmitSignalOnFire();
		}
	}
}
