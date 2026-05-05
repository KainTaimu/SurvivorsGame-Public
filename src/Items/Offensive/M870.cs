using System.Collections.Generic;

namespace Game.Items.Offensive;

public partial class M870 : Firearm
{
	private bool _isShotgunReloading;

	private int PelletCount =>
		Stats.Additional.GetValueOrDefault("PelletCount").AsInt32();

	public override void _Ready()
	{
		base._Ready();
		OnAttack += ApplyCameraRecoil;
	}

	public override void Attack()
	{
		if (_magazineCount <= 0)
		{
			Reload();
			return;
		}
		if (!IsReadyToShoot)
			return;
		_isShotgunReloading = false;

		ShootAudioPlayer?.Play();

		_fireCooldown = Stats.AttackSpeed;
		_magazineCount--;

		var playerVector = Player.GetCanvasTransform() * Player.Position;

		Vector2 mouseVector;
		if (Crosshair is not null)
		{
			mouseVector =
				Crosshair.PrimaryCrosshairSprite.GetCanvasTransform()
				* Crosshair.PrimaryCrosshairSprite.GlobalPosition;
		}
		else
		{
			mouseVector = Player.GetGlobalMousePosition();
		}

		var rotation = playerVector.AngleToPoint(mouseVector);
		for (var i = 0; i < PelletCount; i++)
		{
			var bloomRad = BloomCoefficientDeg * (Math.PI / 180);
			var bloom = (float)GD.RandRange(-bloomRad / 2, bloomRad / 2);

			rotation += bloom;

			var projectile = ProjectilePool.GetProjectile();

			projectile.Origin = this;
			projectile.SetScale(Vector2.One * Stats.ProjectileScaleMultiplier);
			projectile.SetPosition(Player.Position);
			projectile.SetRotation(rotation);
			projectile.ProjectileSpeed =
				Stats.ProjectileSpeed * (float)GD.RandRange(1f, 2f);
			projectile.PierceLimit = Stats.PierceLimit;
			projectile.HitRadius = FirearmStats?.ProjectileRadius ?? 24;
		}
		ApplyCursorRecoil();
		EmitSignalOnAttack();
	}

	public override void Reload()
	{
		if (_isShotgunReloading)
			return;
		if (MagazineCount == MagazineCapacity)
			return;

		_isShotgunReloading = true;
		var tween = CreateTween().SetLoops(MagazineCapacity - MagazineCount);
		tween
			.TweenCallback(
				Callable.From(() =>
				{
					if (!_isShotgunReloading)
					{
						tween.Kill();
						return;
					}
					_magazineCount++;
					ReloadAudioPlayer?.Play();
				})
			)
			.SetDelay(ReloadTimeMs / 1e3);
	}

	public override void _Process(double delta)
	{
		_fireCooldown -= delta;
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

		Attack();
	}
}
