using Game.UI;

namespace Game.Items.Offensive;

public partial class Airstrike : BaseOffensive, IManualAttack
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

		var mousePos = Crosshair.GlobalSpacePosition;

		if (TargetQuery.TryGetTargetsInArea(mousePos, 256, out var targetIds))
		{
			foreach (var id in targetIds)
				HandleHit(id: id);
		}
	}

	protected override void HandleHitECS(int id) { }
}
