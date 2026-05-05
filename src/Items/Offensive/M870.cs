using System.Collections.Generic;

namespace Game.Items.Offensive;

public partial class M870 : Firearm
{
	[Export]
	private AudioStreamPlayer? _shellReloadAudioPlayer;

	[Export]
	private AudioStreamPlayer? _cockingAudioPlayer;

	private bool _isShotgunReloading;
	private double _reloadCooldown;

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
			// Reload();
			return;
		}
		if (!IsReadyToShoot)
			return;
		_isShotgunReloading = false;

		ShootAudioPlayer?.Play();
		if (_cockingAudioPlayer is not null)
			CreateTween()
				.TweenCallback(Callable.From(() => _cockingAudioPlayer.Play()))
				.SetDelay(Stats.AttackSpeed / 1e3 * 0.5f);

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

		var baseRotation = playerVector.AngleToPoint(mouseVector);
		for (var i = 0; i < PelletCount; i++)
		{
			var bloomRad = BloomCoefficientDeg * (Math.PI / 180);
			var bloom = (float)GD.RandRange(-bloomRad / 2, bloomRad / 2);

			var rotation = baseRotation + bloom;

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

	private Tween? _reloadTween;

	public override void Reload()
	{
		if (_reloadCooldown > 0)
			return;
		if (MagazineCount == MagazineCapacity)
			return;

		_magazineCount++;
		_shellReloadAudioPlayer?.Play();
		_reloadCooldown = FirearmStats.ReloadTimeMs / 1e3;

		// _reloadTween?.Kill();
		// _reloadTween = null;
		// _isShotgunReloading = true;
		//
		// // Add delay before reloading sequence to punish reload/shoot spam
		// GetTree().CreateTimer(ReloadTimeMs / 1e3 * 0.7f, false).Timeout += () =>
		// {
		// 	_reloadTween = CreateTween()
		// 		.SetLoops(MagazineCapacity - MagazineCount);
		// 	_reloadTween
		// 		.TweenCallback(
		// 			Callable.From(() =>
		// 			{
		// 				_magazineCount++;
		// 				_shellReloadAudioPlayer?.Play();
		// 				if (_magazineCount == MagazineCapacity)
		// 				{
		// 					_isShotgunReloading = false;
		// 					_reloadTween.Kill();
		// 				}
		// 			})
		// 		)
		// 		.SetDelay(ReloadTimeMs / 1e3);
		// 	_reloadTween.TweenCallback(
		// 		Callable.From(() => _isShotgunReloading = false)
		// 	);
		// };
	}

	public override void _Process(double delta)
	{
		_fireCooldown -= delta;

		if (Input.IsActionPressed(InputMapNames.WeaponReload))
		{
			if (Input.IsActionJustPressed(InputMapNames.WeaponReload))
			{
				_reloadCooldown = 0;
				Reload();
				return;
			}
			_reloadCooldown -= delta;
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
		_reloadCooldown = 0;
	}
}
