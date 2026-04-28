using Game.Core.ECS;
using Game.Items.Projectiles;
using Game.Levels.Controllers;
using Game.UI;

namespace Game.Items.Offensive;

public partial class Airstrike : BaseOffensive
{
	private Crosshair Crosshair => Crosshair.Instance!;
	private EnemyHitManager HitManager => EnemyHitManager.Instance;
	private EntityComponentStore ComponentStore =>
		EntityComponentStore.Instance;

	private double _fireCooldown;

	public override void _Process(double delta)
	{
		_fireCooldown -= delta;
		if (Input.IsActionPressed(InputMapNames.SecondaryAttack))
			Attack();
	}

	protected override void Attack()
	{
		if (_fireCooldown > 0)
			return;
		_fireCooldown = Stats.AttackSpeed;

		var mousePos = Crosshair.GlobalSpacePosition;

		if (!HitManager.TryGetTargetsInArea(mousePos, 256, out var targetIds))
			return;

		foreach (var id in targetIds)
		{
			if (
				!ComponentStore.GetComponent<HealthComponent>(
					id,
					out var health
				)
			)
			{
				continue;
			}
			var crit = CritDamageCalculator.GetCritDamage(
				Stats.Damage,
				Stats.CritDamageMultiplier,
				Stats.CritChanceProportion
			);
			var newHealth = health.Health - Stats.Damage - crit;
			ComponentStore.SetComponent(id, health with { Health = newHealth });

			var hit = new HitFeedbackComponent() { HitTime = 0.5f };
			if (
				!ComponentStore.GetComponent<HitFeedbackComponent>(
					id,
					out var _
				)
			)
				ComponentStore.RegisterComponent(id, hit);
			else
				ComponentStore.SetComponent(id, hit);
		}
	}
}
