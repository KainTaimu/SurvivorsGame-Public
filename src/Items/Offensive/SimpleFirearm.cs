using System.Collections.Generic;
using Arch.Core;
using Game.Levels.Controllers;
using Game.UI;

namespace Game.Items.Offensive;

// TODO: Monolithic class, refactor
/// <summary>
/// A magazine-fed firearm
/// </summary>
public partial class SimpleFirearm : AbstractFirearm, IReloadable, ICustomReloadDisplay
{
	[Signal]
	public delegate void OnReloadStartEventHandler();

	[Signal]
	public delegate void OnReloadEndEventHandler();

	[Signal]
	public delegate void AlmostEmptyEventHandler();

	[Export]
	protected PackedScene _projectileScene = null!;

	[Export]
	protected AbstractFireGroup _fireGroup = null!;

	[Export]
	protected AbstractReloadBehaviour _reloadBehaviour = null!;

	[Export]
	protected AbstractProjectileAttack _projectileAttack = null!;

	[Export]
	private AudioStreamPlayer? _reloadAudioPlayer;

	[Export]
	private AudioStreamPlayer? _boltCloseAudioPlayer;

	public bool IsReloading => _reloadBehaviour.IsReloading;
	public ReloadDisplayType ReloadDisplayType => _reloadBehaviour.DisplayType;

	protected readonly ProjectilePool Pool = new();

	private static Crosshair? Crosshair => Crosshair.Instance;
	private static readonly RandomNumberGenerator _rng = new();

	private bool _fireGroupRegistered;

	public override void _Ready()
	{
		Pool.Initialize(
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
		_fireGroup.ResetState();
		_reloadBehaviour.ResetState();

		if (!_fireGroupRegistered)
		{
			_fireGroup.OnFire += () =>
			{
				if (
					_reloadBehaviour.IsReloading
					&& MagazineCount > 0
					&& _reloadBehaviour is SequentialReloadBehaviour { IsInterrupted: false } seq
				)
					seq.TryInterrupt();
				Attack();
			};
			_fireGroupRegistered = true;
		}

		if (_fireGroup is ICooldown c)
			c.CooldownDuration = FirearmStats.AttackSpeed;
		if (_fireGroup is BurstFireGroup burst)
		{
			if (!Stats.Additional.TryGetValue("BurstCount", out var burstCount))
				Logger.LogError($"{Name} : Key \"BurstCount\" not found in stats");
			burst.BurstCount = burstCount.AsInt32();

			if (!Stats.Additional.TryGetValue("TimeBetweenBurst", out var tbb))
				Logger.LogError($"{Name} : Key \"TimeBetweenBurst\" not found in stats");
			burst.TimeBetweenBursts = tbb.AsSingle();
		}
	}

	private void InitializeReloadBehaviour()
	{
		_fireGroup.ResetState();
		_reloadBehaviour.ResetState();

		_reloadBehaviour.OnReloadStart += () => { };
		_reloadBehaviour.OnReloadEnd += () =>
		{
			MagazineCount = FirearmStats.MagazineCapacity;
			_boltCloseAudioPlayer?.Play();
		};
		_reloadBehaviour.OnReloadEndInterrupted += () =>
		{
			_boltCloseAudioPlayer?.Play();
		};
		_reloadBehaviour.OnReloadProgress += (_, _, step) =>
		{
			MagazineCount += step;
			_reloadAudioPlayer?.Play();
		};
		switch (_reloadBehaviour)
		{
			case NormalReloadBehaviour normalReloadBehaviour:
				normalReloadBehaviour.ReloadTime = FirearmStats.ReloadTime;
				break;
			case SequentialReloadBehaviour sequentialReloadBehaviour:
				sequentialReloadBehaviour.TimeBetweenRounds = FirearmStats.ReloadTime / FirearmStats.MagazineCapacity;
				sequentialReloadBehaviour.RoundsToLoad = FirearmStats.MagazineCapacity;
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(_reloadBehaviour));
		}
	}

	public override void _Process(double delta)
	{
		if (AttackActionString is null)
			return;

		if (_fireGroup is ICooldown fireGroupCooldown)
			fireGroupCooldown.Process((float)delta);

		_reloadBehaviour.Process((float)delta);

		if (Input.IsActionPressed(InputMapNames.WeaponReload))
		{
			Reload();
			return;
		}

		_fireGroup.ProcessInput();
	}

	public virtual bool Attack()
	{
		if (MagazineCount <= 0)
			return false;

		if (_reloadBehaviour.IsReloading)
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
			Pool.GetProjectile,
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

	public virtual void Reload()
	{
		if (IsReloading)
			return;
		if (MagazineCount >= MagazineCapacity)
			return;
		if (_reloadBehaviour is SequentialReloadBehaviour sequentialReloadBehaviour)
			sequentialReloadBehaviour.RoundsToLoad = MagazineCapacity - MagazineCount;
		_reloadBehaviour.Reload();
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
