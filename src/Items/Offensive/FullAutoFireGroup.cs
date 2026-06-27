namespace Game.Items.Offensive;

[GlobalClass]
public partial class FullAutoFireGroup : AbstractFireGroup, ICooldown
{
	public float CooldownDuration { get; set; }
	private float _cooldown;

	private float _timeSinceLastFire;

	public override bool TryFire()
	{
		if (_cooldown > 0)
			return false;

		Reset();
		return true;
	}

	private void Reset()
	{
		_cooldown = CooldownDuration;
		_timeSinceLastFire = Time.GetTicksMsec();
	}

	public void Process(float delta)
	{
		_cooldown = Mathf.Clamp(_cooldown - delta, 0, CooldownDuration);
	}
}
