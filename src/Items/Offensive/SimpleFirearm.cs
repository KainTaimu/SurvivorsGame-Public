using System.Collections.Generic;
using Arch.Core;
using Game.Levels.Controllers;
using Game.UI;

namespace Game.Items.Offensive;

/// <summary>
/// A magazine-fed firearm
/// </summary>
public sealed partial class SimpleFirearm : AbstractFirearm, IReloadable
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

	public bool IsReloading { get; private set; }

	private readonly ProjectilePool _pool = new();

	private static Crosshair? Crosshair => Crosshair.Instance;
	private static readonly RandomNumberGenerator _rng = new();

	public override void _Ready()
	{
		_pool.Initialize(
			this,
			_projectileScene,
			p =>
			{
				p.OnEntityHit += e => TryHandleHit(e.Entity);
			}
		);

		InitializeFireGroupSettings();

		_fireGroup.OnFire += Attack;

		FirearmStats.Changed += InitializeFireGroupSettings;

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
					RecoilAccumilationScale,
					HorizontalRecoilPunish
				);
			}
		};
	}

	private void InitializeFireGroupSettings()
	{
		if (_fireGroup is ICooldown c)
			c.CooldownDuration = FirearmStats.AttackSpeed;
		if (_fireGroup is BurstFireGroup burst)
			burst.TimeBetweenBursts = Stats.Additional["TimeBetweenBurst"].AsSingle();
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

		_fireGroup.ProcessInput();
	}

	public void Attack()
	{
		if (IsReloading || MagazineCount <= 0)
			return;

		if (MagazineCount <= 6)
			EmitSignalAlmostEmpty();

		MagazineCount--;
		if (MagazineCount <= 0)
			Reload();

		var playerPosition = Player.GetCanvasTransform() * Player.Position;

		var mouseVector = Crosshair?.CanvasSpacePosition ?? Player.GetGlobalMousePosition();
		var rotation = playerPosition.AngleToPoint(mouseVector);

		var bloomRad = BloomCoefficientDeg * (Math.PI / 180);
		var bloom = 0.0 + (bloomRad / 2) * _rng.Randfn();
		rotation += (float)bloom;
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
			MagazineCount = MagazineCapacity;
			IsReloading = false;
			EmitSignalOnReloadEnd();
		};
		IsReloading = true;
	}

	protected override void HandleHitECS(Entity entity)
	{
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
