using System.Collections.Generic;
using Arch.Core;
using Game.Levels.Controllers;
using Game.UI;

namespace Game.Items.Offensive;

// TODO: Monolithic class, refactor
/// <summary>
/// A magazine-fed firearm
/// </summary>
public sealed partial class SimpleFirearm : AbstractFirearm, IReloadable, ICustomReloadDisplay
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
	private AbstractFireGroup FireGroup = null!;

	[Export]
	private AbstractReloadBehaviour ReloadBehaviour = null!;

	[Export]
	private AbstractProjectileAttack _projectileAttack = null!;

	[Export]
	private AudioStreamPlayer? _reloadAudioPlayer;

	[Export]
	private AudioStreamPlayer? _boltCloseAudioPlayer;

	public bool IsReloading => ReloadBehaviour.IsReloading;
	public ReloadDisplayType ReloadDisplayType => ReloadBehaviour.DisplayType;

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
		InitializeReloadBehaviour();

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
		FireGroup.OnFire += () =>
		{
			if (
				ReloadBehaviour.IsReloading
				&& MagazineCount > 0
				&& ReloadBehaviour is SequentialReloadBehaviour { IsInterrupted: false } seq
			)
				seq.TryInterrupt();
			Attack();
		};
		if (FireGroup is ICooldown c)
			c.CooldownDuration = FirearmStats.AttackSpeed;
		if (FireGroup is BurstFireGroup burst)
			burst.TimeBetweenBursts = Stats.Additional["TimeBetweenBurst"].AsSingle();
	}

	private void InitializeReloadBehaviour()
	{
		ReloadBehaviour.OnReloadStart += () => { };
		ReloadBehaviour.OnReloadEnd += () =>
		{
			MagazineCount = FirearmStats.MagazineCapacity;
			_boltCloseAudioPlayer?.Play();
		};
		ReloadBehaviour.OnReloadEndInterrupted += () =>
		{
			_boltCloseAudioPlayer?.Play();
		};
		ReloadBehaviour.OnReloadProgress += (_, _, step) =>
		{
			MagazineCount += step;
			_reloadAudioPlayer?.Play();
		};
		switch (ReloadBehaviour)
		{
			case NormalReloadBehaviour normalReloadBehaviour:
				normalReloadBehaviour.ReloadTime = FirearmStats.ReloadTime;
				break;
			case SequentialReloadBehaviour sequentialReloadBehaviour:
				sequentialReloadBehaviour.TimeBetweenRounds = FirearmStats.ReloadTime / FirearmStats.MagazineCapacity;
				sequentialReloadBehaviour.RoundsToLoad = FirearmStats.MagazineCapacity;
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(ReloadBehaviour));
		}
	}

	public override void _Process(double delta)
	{
		if (AttackActionString is null)
			return;

		if (FireGroup is ICooldown fireGroupCooldown)
			fireGroupCooldown.Process((float)delta);

		ReloadBehaviour.Process((float)delta);

		if (Input.IsActionPressed(InputMapNames.WeaponReload))
		{
			Reload();
			return;
		}

		FireGroup.ProcessInput();
	}

	public bool Attack()
	{
		if (MagazineCount <= 0)
			return false;

		if (ReloadBehaviour.IsReloading)
			return false;

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
		return true;
	}

	public void Reload()
	{
		if (IsReloading)
			return;
		if (MagazineCount >= MagazineCapacity)
			return;
		if (ReloadBehaviour is SequentialReloadBehaviour sequentialReloadBehaviour)
			sequentialReloadBehaviour.RoundsToLoad = MagazineCapacity - MagazineCount;
		ReloadBehaviour.Reload();
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
