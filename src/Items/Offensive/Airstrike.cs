using Arch.Core;
using Game.UI;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Game.Items.Offensive;

public partial class Airstrike : BaseOffensive, IHasAttackStats, IHasDamageStats
{
	[Export]
	public DamageStats DamageStats { get; set; } = null!;

	[Export]
	public AttackStats AttackStats { get; set; } = null!;

	private Crosshair Crosshair => Crosshair.Instance!;

	private double _fireCooldown;

	public override void Process(double delta)
	{
		_fireCooldown -= delta;

		if (Input.IsActionPressed(InputMapNames.PrimaryAttack))
			Attack();
	}

	public void Attack()
	{
		if (_fireCooldown > 0)
			return;
		_fireCooldown = AttackStats.AttackSpeed;

		var mousePos = Crosshair.GlobalSpacePosition;

		if (TargetQuery.TryGetTargetsInArea(mousePos, 256, out var targetIds))
		{
			foreach (var entity in targetIds)
				TryHandleHit(entity);
		}
	}

	protected override void HandleDamage(Entity entity)
	{
		OffensiveEffects.ApplyDamage(
			entity,
			DamageStats.Damage,
			DamageStats.CalculateCrit(),
			DamageStats.DamageVariance,
			PlayerStats.OutgoingDamage
		);
	}

	protected override void HandleHitEffects(Entity entity) { }
}
