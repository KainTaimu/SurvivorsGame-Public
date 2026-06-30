using System.Collections.Generic;
using Arch.Core;
using Game.Levels.Controllers;
using Game.Players.Controllers;
using Game.UI;

namespace Game.Items.Offensive;

public partial class Minigun : AbstractFirearm, IReloadable
{
	[Export]
	private PackedScene _projectileScene = null!;

	[Export]
	private AbstractProjectileAttack _projectileAttack = null!;

	private float PlayerPushPerShot => OffensiveStats.Additional.GetValueOrDefault("PlayerPushPerShot").AsSingle();

	[Export]
	private StatusEffect? _movementPenaltyStatusEffect;

	[Export]
	private StatusEffect? _movementPenaltyWearOffStatusEffect;

	private bool _isMovementPenaltyApplied;
	private bool _isMovementPenaltyWearOffApplied;

	public bool IsReloading => false;

	private readonly ProjectilePool _pool = new();

	private static Crosshair? Crosshair => Crosshair.Instance;

	[Export]
	private Curve _windupCurve = null!;

	private double _windupTime;

	private float WindupAttackSpeed =>
		Math.Clamp(_windupCurve.Sample((float)_windupTime), OffensiveStats.AttackSpeed, _windupCurve.MaxDomain);

	private float _fireCooldown;

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
			{
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
			}
		};

		OnEquipped += () =>
		{
			if (_movementPenaltyStatusEffect is null)
				return;
			if (_isMovementPenaltyApplied)
				return;

			Player.StatusEffectController.AddStatusEffect(_movementPenaltyStatusEffect);
			_isMovementPenaltyApplied = true;

			if (_movementPenaltyWearOffStatusEffect is not null && _isMovementPenaltyWearOffApplied)
			{
				Player.StatusEffectController.RemoveStatusEffect(_movementPenaltyWearOffStatusEffect);
				_isMovementPenaltyWearOffApplied = false;
			}
		};
		OnUnequipped += () =>
		{
			if (_movementPenaltyStatusEffect is null)
				return;
			if (!_isMovementPenaltyApplied)
				return;

			Player.StatusEffectController.RemoveStatusEffect(_movementPenaltyStatusEffect);
			_isMovementPenaltyApplied = false;

			if (_movementPenaltyWearOffStatusEffect is not null && !_isMovementPenaltyWearOffApplied)
			{
				Player.StatusEffectController.AddStatusEffect(_movementPenaltyWearOffStatusEffect);
				_isMovementPenaltyWearOffApplied = true;
			}
		};
	}

	public override void _Process(double delta)
	{
		if (AttackActionString is null)
			return;

		if (Input.IsActionPressed(AttackActionString))
		{
			_fireCooldown -= (float)delta;
			_windupTime = Math.Clamp(_windupTime + delta, 0, _windupCurve.MaxDomain);
			Attack();
		}
		else
			_windupTime = Math.Clamp(_windupTime - delta * 3, 0, _windupCurve.MaxDomain);
	}

	public void Attack()
	{
		if (_fireCooldown > 0)
			return;

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

		PushPlayer(rotation, bloom);

		_fireCooldown = WindupAttackSpeed;
		EmitSignalOnAttack();
	}

	public void Reload()
	{
		QueueFree();
	}

	private void PushPlayer(float rotation, float bloom)
	{
		Player.GlobalPosition +=
			Vector2.Right.Rotated(rotation - Mathf.Pi + bloom) * PlayerPushPerShot * (float)GetProcessDeltaTime();
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
