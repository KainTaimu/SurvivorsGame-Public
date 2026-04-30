using System.Collections.Generic;
using Game.Core.ECS;
using Game.UI;

namespace Game.Items.Offensive;

public partial class Revolver : Firearm
{
	[Export]
	private RevolverAmmoCount _ammoCount = null!;

	public override void _Ready()
	{
		base._Ready();
		OnAttack += () =>
		{
			_ammoCount.RotateCylinder();
			ApplyCameraRecoil();
		};
	}

	public override void _Process(double delta)
	{
		_fireCooldown -= delta;
		if (Input.IsActionPressed(InputMapNames.WeaponReload))
		{
			Reload();
			return;
		}
		if (!Input.IsActionJustPressed(InputMapNames.PrimaryAttack))
			return;
		Attack();
	}

	protected override void HandleHitECS(int id)
	{
		base.HandleHitECS(id);

		if (!ComponentStore.GetComponent<PositionComponent>(id, out var pos))
			return;
		var knockback = Stats
			.Additional.GetValueOrDefault("Knockback")
			.AsSingle();
		var knockbackVector = Player.GlobalPosition.DirectionTo(pos.Position);
		knockbackVector *= knockback;

		ComponentStore.SetComponent(
			id,
			pos with
			{
				Position = pos.Position + knockbackVector,
			}
		);
	}
}
