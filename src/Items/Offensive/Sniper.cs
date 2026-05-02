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

	private double MoveTimeFactor =>
		MoveTime
		/ (MoveTimeToMaxBloom != 0 ? MoveTimeToMaxBloom : 1 / double.MaxValue);

	private double MoveTimeToMaxBloom =>
		Stats.Additional["MoveTimeToMaxBloom"].AsDouble();
	private double MoveBloomGrowthRate =>
		Stats.Additional["MoveTimeGrowthRate"].AsDouble();
	private double MoveBloomShrinkRate =>
		Stats.Additional["MoveTimeShrinkRate"].AsDouble();
	private double MoveBloomMinDeg =>
		Stats.Additional["MoveBloomMinDeg"].AsDouble();
	private double MoveBloomMaxDeg =>
		Stats.Additional["MoveBloomMaxDeg"].AsDouble();
	private double MoveDamageMin =>
		Stats.Additional["MoveDamageMin"].AsDouble();
	private double MoveDamageMax =>
		Stats.Additional["MoveDamageMax"].AsDouble();

	private PlayerMovementController? MovementController =>
		GameWorld.Instance.MainPlayer.MovementController;

	public override void _Ready()
	{
		base._Ready();
		OnAttack += () =>
		{
			ApplyCameraRecoil();
			var tween = CreateTween()
				.SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Expo);
			tween.TweenMethod(
				Callable.From<float>(
					(i) =>
					{
						i = Math.Clamp(i, 0, 1);
						SpreadCrosshair(i);
						MoveTime = i;
					}
				),
				1f,
				MoveTime,
				1f
			);
		};
	}

	public override void _Process(double delta)
	{
		_fireCooldown -= delta;

		UpdateMoveTimeBloom(delta);

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

		if (_fireCooldown <= 0)
			Logger.LogInfo(MoveTimeFactor);

		// Stats.Damage = (int)
		// 	Math.Clamp(
		// 		MoveDamageMax * (1 - MoveTimeFactor),
		// 		MoveDamageMin,
		// 		MoveDamageMax
		// 	);

		if (FirearmStats is not null)
			AttackWithMoveTimeBloom(FirearmStats);
		else
			Attack();
	}

	private void AttackWithMoveTimeBloom(FirearmStats firearmStats)
	{
		var bloom = MoveBloomMaxDeg * MoveTimeFactor;
		bloom = Math.Clamp(bloom, MoveBloomMinDeg, MoveBloomMaxDeg);

		firearmStats.BloomCoefficientDeg = (float)bloom;
		Attack();
	}

	private void UpdateMoveTimeBloom(double delta)
	{
		if (
			MovementController?.Velocity.LengthSquared() > 0
			|| Crosshair?.CrosshairVelocity.LengthSquared() > 0
		)
			MoveTime += delta * MoveBloomGrowthRate;
		else
			MoveTime -= delta * MoveBloomShrinkRate;

		SpreadCrosshair((float)MoveTimeFactor);
	}

	private void SpreadCrosshair(float spreadRatio)
	{
		if (AttackActionString is null)
		{
			Crosshair?.ChangePrimaryCrosshairSpread(spreadRatio);
			return;
		}

		if (AttackActionString == InputMapNames.PrimaryAttack)
			Crosshair?.ChangePrimaryCrosshairSpread(spreadRatio);
		else if (AttackActionString == InputMapNames.SecondaryAttack)
			Crosshair?.ChangeSecondaryCrosshairSpread(spreadRatio);
		else
			Crosshair?.ChangePrimaryCrosshairSpread(spreadRatio);
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
