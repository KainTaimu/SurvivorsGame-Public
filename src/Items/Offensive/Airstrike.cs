using Game.Core.ECS;
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
		if (
			Input.IsActionPressed(
				AttackActionString ?? InputMapNames.PrimaryAttack
			)
		)
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
				HandleHitECS(id);
			return;
		}
		// TODO: Handle Node Enemy
	}

	protected override void HandleHitECS(int id)
	{
		if (!ComponentStore.GetComponent<HealthComponent>(id, out var health))
			return;

		var crit = CalculateCrit();
		var newHealth = health.Health - Stats.Damage - crit;

		ComponentStore.SetComponent(id, health with { Health = newHealth });

		var hit = new HitFeedbackComponent() { HitTime = 0.5f };

		if (!ComponentStore.GetComponent<HitFeedbackComponent>(id, out var _))
			ComponentStore.RegisterComponent(id, hit);
		else
			ComponentStore.SetComponent(id, hit);
	}
}
