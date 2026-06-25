using System.Collections.Generic;
using Arch.Core;
using Game.Core.ECS;
using Game.Levels.Controllers;

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

	protected override void HandleHitECS(Entity entity)
	{
		OffensiveEffects.ApplyKnockback(
			entity,
			Player.GlobalPosition,
			OffensiveStats.Additional.GetValueOrDefault("Knockback", 0f).AsSingle()
		);
		OffensiveEffects.ApplyReduceVelocity(
			entity,
			OffensiveStats.Additional.GetValueOrDefault("SlowMultiplier", 1f).AsSingle()
		);
	}
}
