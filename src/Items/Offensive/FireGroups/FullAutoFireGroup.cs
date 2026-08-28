namespace Game.Items.Offensive;

[Tool]
[GlobalClass]
public partial class FullAutoFireGroup : AbstractFireGroup, ICooldown
{
	public float CooldownDuration { get; set; }
	private float _cooldown;

	public override void ProcessInput()
	{
		if (_cooldown > 0)
			return;

		if (!Input.IsActionPressed(InputMapNames.PrimaryAttack))
			return;

		_cooldown = CooldownDuration;
		EmitSignalOnFire();
	}

	public void Process(float delta)
	{
		_cooldown = Mathf.Clamp(_cooldown - delta, 0, CooldownDuration);
	}
}
