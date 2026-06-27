namespace Game.Items.Offensive;

[GlobalClass]
public partial class SemiAutoFireGroup : AbstractFireGroup, ICooldown, IFireQueuable
{
	public float CooldownDuration { get; set; }

	public bool CanFireQueued { get; private set; }

	private bool _isFireQueued;
	private ulong _ticksSinceLastFire;
	private float _cooldown;

	// Allow queuing a shot if _cooldown is FIRE_QUEUE_TOLERANCE% of CooldownDuration
	[Export]
	private float _fireQueueTolerance = 0.5f;

	public override bool TryFire()
	{
		if (CanFireQueued)
		{
			ResetOnFire();
			return true;
		}
		if (!Input.IsActionJustPressed(InputMapNames.PrimaryAttack))
			return false;

		var inQueueWindow = _cooldown <= CooldownDuration * _fireQueueTolerance;
		if (inQueueWindow && !_isFireQueued)
			_isFireQueued = true;

		var shouldFire = _cooldown <= 0;
		if (shouldFire)
			ResetOnFire();

		return shouldFire;
	}

	private void ResetOnFire()
	{
		_cooldown = CooldownDuration;
		_isFireQueued = false;
		_ticksSinceLastFire = Time.GetTicksMsec();
		CanFireQueued = false;
	}

	public void Process(float delta)
	{
		_cooldown = Mathf.Clamp(_cooldown - delta, 0, CooldownDuration);
		if (_cooldown <= 0 && _isFireQueued)
			CanFireQueued = true;
	}
}
