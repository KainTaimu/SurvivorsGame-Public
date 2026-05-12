using System.Collections.Generic;
using Game.Core.ECS;
using Game.Players.Controllers;

namespace Game.Items.Offensive;

public partial class Minigun : Firearm
{
	[Export]
	private Curve _windupCurve = null!;

	private double _windupTime;
	private float WindupAttackSpeed =>
		Math.Clamp(_windupCurve.Sample((float)_windupTime), OffensiveStats.AttackSpeed, _windupCurve.MaxDomain);

	private float PlayerPushPerShot => OffensiveStats.Additional.GetValueOrDefault("PlayerPushPerShot").AsSingle();

	[Export]
	private StatusEffect? _movementPenaltyStatusEffect;

	private bool _isMovementPenaltyApplied;

	public override void _Ready()
	{
		base._Ready();
		OnAttack += ApplyCameraRecoil;

		OnEquipped += () =>
		{
			if (_movementPenaltyStatusEffect is null || _isMovementPenaltyApplied)
				return;
			Player.StatusEffectController.AddStatusEffect(_movementPenaltyStatusEffect);
			_isMovementPenaltyApplied = true;
		};
		OnUnequipped += () =>
		{
			if (_movementPenaltyStatusEffect is not null && _isMovementPenaltyApplied)
			{
				Player.StatusEffectController.RemoveStatusEffect(_movementPenaltyStatusEffect);
				_isMovementPenaltyApplied = false;
			}
		};
	}

	public override void _Process(double delta)
	{
		if (AttackActionString is null)
			return;

		if (Input.IsActionPressed(AttackActionString))
		{
			FireCooldown -= delta;
			_windupTime = Math.Clamp(_windupTime + delta, 0, _windupCurve.MaxDomain);
			Attack();
		}
		else
		{
			_windupTime = Math.Clamp(_windupTime - delta * 3, 0, _windupCurve.MaxDomain);
		}
	}

	public override void Attack()
	{
		if (FireCooldown > 0)
			return;

		ShootAudioPlayer?.Play();

		FireCooldown = WindupAttackSpeed;
		MagazineCount--;

		if (MagazineCount == 0)
			Reload();

		var playerVector = Player.GetCanvasTransform() * Player.Position;

		Vector2 mouseVector;
		if (Crosshair is not null)
		{
			mouseVector =
				Crosshair.PrimaryCrosshairSprite.GetCanvasTransform() * Crosshair.PrimaryCrosshairSprite.GlobalPosition;
		}
		else
			mouseVector = Player.GetGlobalMousePosition();

		var rotation = playerVector.AngleToPoint(mouseVector);

		var bloomRad = BloomCoefficientDeg * (Math.PI / 180);
		var bloom = (float)GD.RandRange(-bloomRad / 2, bloomRad / 2);

		rotation += bloom;

		var projectile = ProjectilePool.GetProjectile();

		projectile.Origin = this;
		projectile.SetScale(Vector2.One * OffensiveStats.ProjectileScaleMultiplier);
		projectile.SetPosition(Player.Position);
		projectile.SetRotation(rotation);
		projectile.ProjectileSpeed = OffensiveStats.ProjectileSpeed;
		projectile.ProjectileSpeed += (float)
			GD.RandRange(-projectile.ProjectileSpeed * 0.2, projectile.ProjectileSpeed * 0.2);
		projectile.PierceLimit = OffensiveStats.PierceLimit;
		projectile.HitRadius = FirearmStats.ProjectileRadius;
		projectile.Initialize();

		Player.GlobalPosition +=
			Vector2.Right.Rotated(rotation - Mathf.Pi + bloom) * PlayerPushPerShot * (float)GetProcessDeltaTime();

		ApplyCursorRecoil();
		SpawnCasingParticle();
		EmitSignalOnAttack();
	}

	public override void Reload()
	{
		QueueFree();
	}

	protected override void HandleHitECS(int id)
	{
		if (!ComponentStore.GetComponent<PositionComponent>(id, out var pos))
			return;
		var knockback = OffensiveStats.Additional.GetValueOrDefault("Knockback").AsSingle();
		var knockbackVector = Player.GlobalPosition.DirectionTo(pos.Position);
		knockbackVector *= knockback;

		ComponentStore.SetComponent(id, new PositionComponent(pos.Position + knockbackVector));
	}
}
