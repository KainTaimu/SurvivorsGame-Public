using Arch.Core;
using Game.Core.ECS;
using Game.Levels.Controllers;
using Game.UI;

namespace Game.Items.Offensive;

public partial class GrenadeLauncher
	: BaseOffensive,
		IReloadable,
		ICustomReloadDisplay,
		IHasDamageStats,
		IHasMagazineStats,
		IHasBloomStats,
		IHasRecoilStats,
		IHasProjectileStats,
		IHasAttackStats,
		IHasGrenadeStats
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
	public GrenadeStats GrenadeStats { get; set; } = null!;

	[ExportCategory("")]
	[Export]
	private float _throwForce = 1250;

	[Export]
	private Curve _damageDropoffFromCenter = null!;

	[Export]
	public PackedScene GrenadeScene = null!;

	[Export]
	private AbstractFireGroup _fireGroup = null!;

	[Export]
	private SequentialReloadBehaviour _reloadBehaviour = null!;

	[Export]
	private AudioStreamPlayer? _explosionPlayer;

	private Crosshair? Crosshair => Crosshair.Instance;

	public bool IsReloading => _reloadBehaviour.IsReloading;
	public ReloadDisplayType ReloadDisplayType => _reloadBehaviour.DisplayType;

	public int MagazineCapacity => MagazineStats.Capacity;

	public int MagazineCount
	{
		get => MagazineStats.Count;
		set => MagazineStats.Count = value;
	}

	private Vector2 _lastBlastPosition;

	public override void _Ready()
	{
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
		_fireGroup.OnFire += () =>
		{
			_reloadBehaviour.TryInterrupt();
			Attack();
		};
	}

	private void InitializeReloadBehaviour()
	{
		_reloadBehaviour.OnReloadEnd += () =>
		{
			MagazineCount = MagazineStats.Capacity;
		};

		_reloadBehaviour.OnReloadProgress += (_, _, step) =>
		{
			MagazineCount += step;
		};
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

	private void Attack()
	{
		if (MagazineCount <= 0)
			return;

		if (_reloadBehaviour.IsReloading)
			return;

		if (Crosshair is null)
		{
			CustomLogger.LogError("No crosshair");
			return;
		}

		MagazineCount--;
		if (MagazineCount == 0)
			Reload();

		var nade = GrenadeScene.Instantiate<Grenade>();
		nade.OnExploded += (blastPos) =>
		{
			_explosionPlayer?.Reparent(GetTree().Root);
			_explosionPlayer?.Play();
			_lastBlastPosition = blastPos;
		};

		nade.OffensiveOrigin = this;
		nade.GlobalPosition = Player.GlobalPosition;
		nade.ArmingDistance = GrenadeStats.ArmingDistance;
		nade.CameraRecoilScale = RecoilStats.CameraRecoilScale;
		nade.EnemiesInAreaUntilExplode = GrenadeStats.EnemiesInAreaUntilExplode;
		nade.TimeToExplode = GrenadeStats.TimeToExplode;

		var mouseVector = Crosshair?.GlobalSpacePosition ?? Player.GetGlobalMousePosition();

		var bloomRad = BloomStats.BloomCoefficientDeg * (Math.PI / 180);
		var bloom = (float)GD.RandRange(-bloomRad / 2, bloomRad / 2);

		var force = Vector2.Right.Rotated(nade.GlobalPosition.AngleToPoint(mouseVector) + bloom) * _throwForce;
		nade.LinearVelocity = force;
		GetTree().Root.Call(Window.MethodName.AddChild, nade);
		EmitSignalOnAttack();
	}

	public void Reload()
	{
		_reloadBehaviour.Reload();
	}

	protected override void HandleDamage(Entity entity)
	{
		ref var pos = ref GameWorld.World.Get<PositionComponent>(entity);

		var offset = _lastBlastPosition.DistanceTo(pos.Position) / ProjectileStats.HitRadius;
		offset = Mathf.Clamp(offset, 0, 1);
		var dropoffScale = _damageDropoffFromCenter.Sample(offset);

		OffensiveEffects.ApplyDamage(
			entity,
			Mathf.CeilToInt(DamageStats.Damage * dropoffScale),
			DamageStats.CalculateCrit(),
			DamageStats.DamageVariance,
			PlayerStats.OutgoingDamage
		);
	}

	protected override void HandleHitEffects(Entity entity)
	{
		OffensiveEffects.ApplyVelocityMultiplier(entity, 0f);

		var health = GameWorld.World.Get<HealthComponent>(entity);
		if (health.Health > 0)
			return;
		GameWorld.World.Add(entity, new DeathCauseComponent(DeathCauseEnum.Explosion));
	}
}
