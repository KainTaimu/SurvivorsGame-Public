using System.Collections.Generic;
using Arch.Core;
using Game.Levels.Controllers;
using Game.UI;

namespace Game.Items.Offensive;

public partial class Shotgun : AbstractFirearm, IReloadable
{
	[Export]
	private PackedScene _projectileScene = null!;

	[Export]
	private AbstractFireGroup _fireGroup = null!;

	[Export]
	private AbstractProjectileAttack _projectileAttack = null!;

	[Export]
	private AudioStreamPlayer? _shellReloadAudioPlayer;

	[Export]
	private AudioStreamPlayer? _cockingAudioPlayer;

	private double _reloadCooldown;

	private int PelletCount => OffensiveStats.Additional.GetValueOrDefault("PelletCount").AsInt32();

	private readonly ProjectilePool _pool = new();

	private static Crosshair? Crosshair => Crosshair.Instance;
	public bool IsReloading { get; private set; }

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
	}

	public override void _Process(double delta)
	{
		if (AttackActionString is null)
			return;

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

		if (_fireGroup.TryFire())
			Attack();
	}

	public void Attack()
	{
		if (MagazineCount <= 0)
		{
			Reload();
			return;
		}

		MagazineCount--;

		if (_cockingAudioPlayer is not null)
			GetTree().CreateTimer(OffensiveStats.AttackSpeed / 2).Timeout += () => _cockingAudioPlayer.Play();

		var playerPosition = Player.GetCanvasTransform() * Player.Position;

		var mouseVector = Crosshair?.CanvasSpacePosition ?? Player.GetGlobalMousePosition();

		var baseRotation = playerPosition.AngleToPoint(mouseVector);
		for (var i = 0; i < PelletCount; i++)
		{
			var bloomRad = BloomCoefficientDeg * (Math.PI / 180);
			var bloom = (float)GD.RandRange(-bloomRad / 2, bloomRad / 2);
			var rotation = baseRotation + bloom;
			var scale = Vector2.One * FirearmStats.ProjectileScaleMultiplier;
			var speed = OffensiveStats.ProjectileSpeed * (float)GD.RandRange(1f, 2f);

			_projectileAttack.Attack(
				_pool.GetProjectile,
				Player.GlobalPosition,
				rotation,
				FirearmStats.ProjectileRadius,
				speed,
				FirearmStats.PierceLimit,
				scale
			);
		}

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

	protected override void HandleHitECS(Entity entity)
	{
		OffensiveEffects.ApplyDamage(
			entity,
			FirearmStats.Damage,
			CalculateCrit(),
			FirearmStats.DamageVarianceMultiplier,
			PlayerStats.OutgoingDamageMultiplier
		);
		OffensiveEffects.ApplyKnockback(
			entity,
			Player.GlobalPosition,
			OffensiveStats.Additional.GetValueOrDefault("Knockback", 0f).AsSingle()
		);
		OffensiveEffects.ApplyVelocityMultiplier(
			entity,
			OffensiveStats.Additional.GetValueOrDefault("SlowMultiplier", 1f).AsSingle()
		);
	}
}
