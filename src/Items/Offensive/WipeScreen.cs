using Arch.Core;
using Game.UI;

namespace Game.Items.Offensive;

public partial class WipeScreen : BaseOffensive, IHasAttackStats, IHasDamageStats
{
	[ExportCategory("Stats")]
	[Export]
	public DamageStats DamageStats { get; set; } = null!;

	[Export]
	public AttackStats AttackStats { get; set; } = null!;

	private Crosshair Crosshair => Crosshair.Instance!;

	private double _fireCooldown;

	public override void _Process(double delta)
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

		foreach (var entity in TargetQuery.GetTargetsInScreen())
			TryHandleHit(entity);
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
