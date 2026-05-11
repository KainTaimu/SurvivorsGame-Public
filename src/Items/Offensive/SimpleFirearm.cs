using System.Collections.Generic;
using Game.Core.ECS;

namespace Game.Items.Offensive;

public partial class SimpleFirearm : Firearm
{
	public override void _Ready()
	{
		base._Ready();
		OnAttack += ApplyCameraRecoil;
	}

	public override void _Process(double delta)
	{
		FireCooldown -= delta;
		if (AttackActionString is null)
			return;

		if (Input.IsActionPressed(InputMapNames.WeaponReload))
		{
			Reload();
			return;
		}

		switch (FirearmStats.FireGroup)
		{
			case FireGroup.Single:
				if (!Input.IsActionJustPressed(AttackActionString))
					return;
				break;
			case FireGroup.Burst:
				throw new NotImplementedException();
			case FireGroup.Auto:
				if (!Input.IsActionPressed(AttackActionString))
					return;
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}

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
