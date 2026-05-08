using System.Collections.Generic;
using Game.Core.ECS;

namespace Game.Items.Offensive;

// ReSharper disable once InconsistentNaming
public partial class TP9 : Firearm
{
	public override void _Ready()
	{
		base._Ready();
		OnAttack += ApplyCameraRecoil;
	}

	public override void _Process(double delta)
	{
		FireCooldown -= delta;
		if (Input.IsActionPressed(InputMapNames.WeaponReload))
		{
			Reload();
			return;
		}

		if (!Input.IsActionPressed(AttackActionString ?? InputMapNames.PrimaryAttack))
			return;

		Attack();
	}

	protected override void HandleHitECS(int id)
	{
		if (!ComponentStore.GetComponent<PositionComponent>(id, out var pos))
			return;
		var knockback = Stats.Additional.GetValueOrDefault("Knockback").AsSingle();
		var knockbackVector = Player.GlobalPosition.DirectionTo(pos.Position);
		knockbackVector *= knockback;

		ComponentStore.SetComponent(id, new PositionComponent(pos.Position + knockbackVector));
	}
}
