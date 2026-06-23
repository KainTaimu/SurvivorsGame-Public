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
		if (!GameWorld.World.TryGet<PositionComponent>(entity, out var pos))
			return;
		var knockback = OffensiveStats.Additional.GetValueOrDefault("Knockback").AsSingle();
		var knockbackVector = Player.GlobalPosition.DirectionTo(pos.Position);
		knockbackVector *= knockback;

		GameWorld.World.Set(entity, new PositionComponent(pos.Position + knockbackVector));
	}
}
