using System.Collections.Generic;
using Game.Core.ECS;
using Game.Levels.Controllers;
using Game.Players.Controllers;

namespace Game.Items.Offensive;

public partial class Sniper : Firearm
{
	private double MoveTime
	{
		get;
		set { field = Math.Clamp(value, 0, MoveTimeToMaxBloom); }
	}

	private double MoveTimeToMaxBloom =>
		Stats.Additional["MoveTimeToMaxBloom"].AsDouble();
	private double MoveTimeBloomGrowthRate =>
		Stats.Additional["MoveTimeBloomGrowthRate"].AsDouble();
	private double MoveTimeBloomShrinkRate =>
		Stats.Additional["MoveTimeBloomShrinkRate"].AsDouble();

	private PlayerMovementController? MovementController =>
		GameWorld.Instance.MainPlayer.MovementController;

	public override void _Ready()
	{
		base._Ready();
		OnAttack += ApplyCameraRecoil;
	}

	public override void _Process(double delta)
	{
		_fireCooldown -= delta;

		if (MovementController?.Velocity.LengthSquared() > 0)
			MoveTime += delta * MoveTimeBloomGrowthRate;
		else
			MoveTime -= delta * MoveTimeBloomShrinkRate;

		if (AttackActionString is not null)
		{
			if (AttackActionString == InputMapNames.PrimaryAttack)
			{
				Crosshair?.ChangePrimaryCrosshairSpread(
					(float)(
						MoveTime
						/ (MoveTimeToMaxBloom != 0 ? MoveTimeToMaxBloom : 1e-10)
					)
				);
			}
			else if (AttackActionString == InputMapNames.SecondaryAttack)
			{
				Crosshair?.ChangeSecondaryCrosshairSpread(
					(float)(
						MoveTime
						/ (MoveTimeToMaxBloom != 0 ? MoveTimeToMaxBloom : 1e-10)
					)
				);
			}
		}

		if (Input.IsActionPressed(InputMapNames.WeaponReload))
		{
			Reload();
			return;
		}
		if (
			!Input.IsActionPressed(
				AttackActionString ?? InputMapNames.PrimaryAttack
			)
		)
			return;

		var orig = FirearmStats?.BloomCoefficientDeg ?? 0;
		FirearmStats?.BloomCoefficientDeg = (float)(
			orig
			* (
				MoveTime
				/ (MoveTimeToMaxBloom != 0 ? MoveTimeToMaxBloom : 1e-10)
			)
		);
		Attack();
		FirearmStats?.BloomCoefficientDeg = orig;
	}

	protected override void HandleHitECS(int id)
	{
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
