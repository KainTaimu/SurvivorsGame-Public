namespace Game.Items.Offensive;

[GlobalClass]
public partial class FullAutoFireGroup : AbstractFireGroup, ICooldown
{
	[Export]
	private AttackStats AttackStats { get; set; } = null!;

	private float CooldownDuration => AttackStats.AttackSpeed;
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
