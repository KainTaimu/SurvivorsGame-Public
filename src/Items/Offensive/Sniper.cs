using System.Collections.Generic;
using Arch.Core;
using Game.Core.ECS;
using Game.Levels.Controllers;
using Game.Players.Controllers;

namespace Game.Items.Offensive;

public partial class Sniper : Firearm
{
	private double MoveTime
	{
		get;
		set => field = Math.Clamp(value, 0, MoveTimeToMaxBloom);
	}

	private double MoveTimeFactor => MoveTime / (MoveTimeToMaxBloom != 0 ? MoveTimeToMaxBloom : 1 / double.MaxValue);

	private double MoveTimeToMaxBloom => Stats.Additional["MoveTimeToMaxBloom"].AsDouble();

	private double MoveBloomGrowthRate => Stats.Additional["MoveTimeGrowthRate"].AsDouble();

	private double MoveBloomShrinkRate => Stats.Additional["MoveTimeShrinkRate"].AsDouble();

	private double MoveBloomMinDeg => Stats.Additional["MoveBloomMinDeg"].AsDouble();

	private double MoveBloomMaxDeg => Stats.Additional["MoveBloomMaxDeg"].AsDouble();

	private int MoveDamageMin => Stats.Additional["MoveDamageMin"].AsInt32();

	private int MoveDamageMax => Stats.Additional["MoveDamageMax"].AsInt32();

	private PlayerMovementController MovementController => GameWorld.Instance.MainPlayer.MovementController;

	private bool _isFireQueued;

	public override void _Ready()
	{
		base._Ready();
		OnAttack += ApplyCameraRecoil;

		if (OffensiveStats.Damage > 0)
		{
			Logger.LogError(
				$"Sniper {Name} has base damage {OffensiveStats.Damage}, but it will be overridden by move time damage. Consider setting base damage to 0."
			);
		}
	}

	public override void _Process(double delta)
	{
		FireCooldown -= delta;
		if (AttackActionString is null)
			return;

		UpdateMoveTimeBloom(delta);

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

		if (IsReadyToShoot)
		{
			OffensiveStats.BaseCritChanceProportion = (float)(1 - MoveTimeFactor);
			OffensiveStats.BaseDamage = (int)
				Math.Clamp(MoveDamageMax * (1 - MoveTimeFactor), MoveDamageMin, MoveDamageMax);
		}

		AttackWithMoveTimeBloom(FirearmStats);
	}

	private void AttackWithMoveTimeBloom(FirearmStats firearmStats)
	{
		var bloom = MoveBloomMaxDeg * MoveTimeFactor;
		bloom = Math.Clamp(bloom, MoveBloomMinDeg, MoveBloomMaxDeg);

		firearmStats.BloomCoefficientDeg = (float)bloom;

		if (FireCooldown <= 0)
		{
			Attack();
			_isFireQueued = false;
			return;
		}

		if (_isFireQueued)
			return;

		_isFireQueued = true;
		GetTree().CreateTimer(FireCooldown, false).Timeout += () =>
		{
			Attack();
			_isFireQueued = false;
		};
	}

	private void UpdateMoveTimeBloom(double delta)
	{
		if (MovementController.Velocity.LengthSquared() > 0 || Crosshair?.CrosshairVelocity.LengthSquared() > 0)
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
