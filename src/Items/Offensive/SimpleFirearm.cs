using Arch.Core;
using Game.Levels.Controllers;
using Game.UI;

namespace Game.Items.Offensive;

/// <summary>
/// A magazine-fed firearm
/// </summary>
public partial class SimpleFirearm
	: BaseOffensive,
		IReloadable,
		ICustomReloadDisplay,
		IHasDamageStats,
		IHasMagazineStats,
		IHasBloomStats,
		IHasRecoilStats,
		IHasProjectileStats,
		IHasAttackStats,
		IHasPierceStats,
		IHasSlowStats,
		IHasKnockbackStats,
		IMayHasBurstFireStats
{
	[Signal]
	public delegate void OnReloadStartEventHandler();

	[Signal]
	public delegate void OnReloadEndEventHandler();

	[Signal]
	public delegate void AlmostEmptyEventHandler();

	[ExportCategory("Stats")]
	[Export]
	public DamageStats DamageStats { get; set; } = null!;

	[Export]
	public MagazineStats MagazineStats { get; set; } = null!;

	[Export]
	public BloomStats BloomStats { get; set; } = null!;

	[Export]
	public RecoilStats RecoilStats { get; set; } = null!;

	[Export]
	public ReloadStats ReloadStats { get; set; } = null!;

	[Export]
	public ProjectileStats ProjectileStats { get; set; } = null!;

	[Export]
	public AttackStats AttackStats { get; set; } = null!;

	[Export]
	public PierceStats PierceStats { get; set; } = null!;

	[Export]
	public SlowStats SlowStats { get; set; } = null!;

	[Export]
	public KnockbackStats KnockbackStats { get; set; } = null!;

	[Export]
	public BurstFireStats? BurstFireStats { get; set; }

	[ExportCategory("")]
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

	public int MagazineCapacity => MagazineStats.Capacity;

	public int MagazineCount
	{
		get => MagazineStats.Count;
		set => MagazineStats.Count = value;
	}

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

		OnAttack += () => OffensiveEffects.ApplyCameraShake(RecoilStats.CameraRecoilScale, GetViewport, CreateTween);
		OnAttack += () =>
		{
			if (Crosshair is not null)
			{
				OffensiveEffects.ApplyCrosshairRecoil(
					Crosshair,
					RecoilStats.HorizontalBaseRecoil,
					RecoilStats.HorizontalRecoilMin,
					RecoilStats.HorizontalRecoilRandom,
					RecoilStats.VerticalBaseRecoil,
					RecoilStats.VerticalRecoilMin,
					RecoilStats.VerticalRecoilRandom,
					RecoilStats.RecoilScale,
					RecoilStats.RecoilAccumilationScale,
					RecoilStats.HorizontalRecoilPunish
				);
			}
		};
	}

	private void InitializeFireGroupSettings()
	{
		if (_fireGroupRegistered)
			return;
		_fireGroup.OnFire += () =>
		{
			(_reloadBehaviour as IInterruptableReloadBehaviour)?.TryInterrupt();
			Attack();
		};
		_fireGroupRegistered = true;
	}

	private void InitializeReloadBehaviour()
	{
		_reloadBehaviour.OnReloadStart += () => { };
		_reloadBehaviour.OnReloadEnd += () =>
		{
			MagazineCount = MagazineStats.Capacity;
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
				normalReloadBehaviour.ReloadTime = () => ReloadStats.ReloadTime;
				break;
			case SequentialReloadBehaviour sequentialReloadBehaviour:
				sequentialReloadBehaviour.TimeBetweenRounds = () => ReloadStats.ReloadTime / MagazineStats.Capacity;
				sequentialReloadBehaviour.RoundsToLoad = MagazineStats.Capacity;
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(_reloadBehaviour));
		}
	}

	public override void Process(double delta)
	{
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

		var bloomRad = BloomStats.BloomCoefficientDeg * (Math.PI / 180);
		var bloom = 0.0 + bloomRad / 2 * _rng.Randfn();
		rotation += (float)bloom;
		var scale = Vector2.One * ProjectileStats.Scale;

		_projectileAttack.Attack(
			Pool.GetProjectile,
			Player.GlobalPosition,
			rotation,
			ProjectileStats.HitRadius,
			ProjectileStats.Speed,
			PierceStats.PierceLimit,
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

	protected override void HandleDamage(Entity entity)
	{
		OffensiveEffects.ApplyDamage(
			entity,
			DamageStats.Damage,
			DamageStats.CalculateCrit(),
			DamageStats.DamageVariance,
			PlayerStats.OutgoingDamage
		);
	}

	protected override void HandleHitEffects(Entity entity)
	{
		OffensiveEffects.ApplyKnockback(entity, Player.GlobalPosition, KnockbackStats.KnockbackForce);
		OffensiveEffects.ApplyVelocityMultiplier(entity, SlowStats.SlowCoefficient);
	}
}
