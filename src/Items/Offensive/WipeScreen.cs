using Game.UI;

namespace Game.Items.Offensive;

public partial class WipeScreen : BaseOffensive, IManualAttack
{
	private Crosshair Crosshair => Crosshair.Instance!;

	public string? AttackActionString { get; set; }

	private double _fireCooldown;

	public override void _Process(double delta)
	{
		_fireCooldown -= delta;
		if (AttackActionString is null)
			return;

		if (Input.IsActionPressed(AttackActionString))
			Attack();
	}

	public override void Attack()
	{
		if (_fireCooldown > 0)
			return;
		_fireCooldown = Stats.AttackSpeed;

		foreach (var id in TargetQuery.GetTargetsInScreen())
			HandleHit(id: id);
	}

	protected override void HandleHitECS(int id) { }
}
