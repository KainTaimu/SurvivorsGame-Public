using System.Collections.Generic;
using Arch.Core;
using Game.Levels.Controllers;
using Game.Players.Controllers;
using Game.UI;

namespace Game.Items.Offensive;

public interface IBloomable { }

public partial class Sniper : AbstractFirearm, IReloadable
{
	[Signal]
	public delegate void OnReloadStartEventHandler();

	[Signal]
	public delegate void OnReloadEndEventHandler();

	[Signal]
	public delegate void AlmostEmptyEventHandler();

	[Export]
	private PackedScene _projectileScene = null!;

	[Export]
	private AbstractFireGroup _fireGroup = null!;

	[Export]
	private AbstractProjectileAttack _projectileAttack = null!;

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

	public bool IsReloading { get; private set; }

	private readonly ProjectilePool _pool = new();

	private static Crosshair? Crosshair => Crosshair.Instance;

	public override void _Ready()
	{
		_pool.Initialize(
			this,
			_projectileScene,
			p =>
			{
				p.OnEntityHit += e => HandleHit(e.Entity);
			}
		);

		OnAttack += () => OffensiveEffects.ApplyCameraShake(FirearmStats.CameraRecoilScale, GetViewport, CreateTween);
		OnAttack += () =>
		{
			if (Crosshair is not null)
				OffensiveEffects.ApplyCrosshairRecoil(
					Crosshair,
					HorizontalBaseRecoil,
					HorizontalRecoilMin,
					HorizontalRecoilRandom,
					VerticalBaseRecoil,
					VerticalRecoilMin,
					VerticalRecoilRandom,
					RecoilScale,
					RecoilAccumilationScale
				);
		};

		if (OffensiveStats.Damage > 0)
		{
			Logger.LogError(
				$"Sniper {Name} has base damage {OffensiveStats.Damage}, but it will be overridden by move time damage. Consider setting base damage to 0."
			);
		}
	}

	public override void _Process(double delta)
	{
		if (AttackActionString is null)
			return;

		UpdateMoveTimeBloom(delta);
		if (_fireGroup is ICooldown fireGroupCooldown)
			fireGroupCooldown.Process((float)delta);

		if (Input.IsActionPressed(InputMapNames.WeaponReload))
		{
			Reload();
			return;
		}

		if (IsReloading)
			return;

		if (_fireGroup is IFireQueuable { CanFireQueued: true })
		{
			if (!_fireGroup.TryFire())
				return;
			Attack();
			return;
		}

		if (!Input.IsActionPressed(AttackActionString))
			return;

		if (!_fireGroup.TryFire())
			return;

		OffensiveStats.BaseCritChanceProportion = (float)(1 - MoveTimeFactor);
		OffensiveStats.BaseDamage = (int)Math.Clamp(MoveDamageMax * (1 - MoveTimeFactor), MoveDamageMin, MoveDamageMax);

		var bloom = MoveBloomMaxDeg * MoveTimeFactor;
		bloom = Math.Clamp(bloom, MoveBloomMinDeg, MoveBloomMaxDeg);

		FirearmStats.BloomCoefficientDeg = (float)bloom;

		Attack();
	}

	public void Attack()
	{
		if (MagazineCount <= 6)
			EmitSignalAlmostEmpty();

		MagazineCount--;
		if (MagazineCount == 0)
			Reload();

		var playerPosition = Player.GetCanvasTransform() * Player.Position;

		var mouseVector = Crosshair?.CanvasSpacePosition ?? Player.GetGlobalMousePosition();
		var rotation = playerPosition.AngleToPoint(mouseVector);

		var bloomRad = BloomCoefficientDeg * (Math.PI / 180);
		var bloom = (float)GD.RandRange(-bloomRad / 2, bloomRad / 2);
		rotation += bloom;
		var scale = Vector2.One * FirearmStats.ProjectileScaleMultiplier;

		_projectileAttack.Attack(
			_pool.GetProjectile,
			Player.GlobalPosition,
			rotation,
			FirearmStats.ProjectileRadius,
			FirearmStats.ProjectileSpeed,
			FirearmStats.PierceLimit,
			scale
		);

		EmitSignalOnAttack();
	}

	public void Reload()
	{
		if (IsReloading)
			return;
		if (MagazineCount >= MagazineCapacity)
			return;
		GetTree().CreateTimer(FirearmStats.ReloadTime, false).Timeout += () =>
		{
			if (MagazineCount == 0)
				MagazineCount = MagazineCapacity;
			else
				MagazineCount = MagazineCapacity + 1; // Round in chamber
			IsReloading = false;
		};
		IsReloading = true;
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
		OffensiveEffects.ApplyKnockback(
			entity,
			Player.GlobalPosition,
			OffensiveStats.Additional.GetValueOrDefault("Knockback", 0f).AsSingle()
		);
	}
}
