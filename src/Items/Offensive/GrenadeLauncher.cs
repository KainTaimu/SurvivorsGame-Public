using Arch.Core;
using Game.Core.ECS;
using Game.Levels.Controllers;
using Game.UI;

namespace Game.Items.Offensive;

public partial class GrenadeLauncher : BaseOffensive, IManualAttack, IReloadable, ICustomReloadDisplay
{
	[Signal]
	public delegate void OnReloadStartEventHandler();

	[Signal]
	public delegate void OnReloadEndEventHandler();

	[Signal]
	public delegate void AlmostEmptyEventHandler();

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

	public FirearmStats FirearmStats => (FirearmStats)OffensiveStats;
	private Crosshair? Crosshair => Crosshair.Instance;

	public string? AttackActionString { get; set; }

	public bool IsReloading => _reloadBehaviour.IsReloading;
	public ReloadDisplayType ReloadDisplayType => _reloadBehaviour.DisplayType;

	public int MagazineCapacity => FirearmStats.MagazineCapacity;

	public int MagazineCount
	{
		get
		{
			// YUCK
			if (field == int.MinValue)
				field = MagazineCapacity;
			return field;
		}
		set => field = field == int.MinValue ? MagazineCapacity : value;
	} = int.MinValue;

	private Vector2 _lastBlastPosition;

	public override void _Ready()
	{
		if (_fireGroup is ICooldown c)
			c.CooldownDuration = FirearmStats.AttackSpeed;

		InitializeFireGroupSettings();
		InitializeReloadBehaviour();

		OnAttack += () => OffensiveEffects.ApplyCameraShake(FirearmStats.CameraRecoilScale, GetViewport, CreateTween);
		OnAttack += () =>
		{
			if (Crosshair is not null)
			{
				OffensiveEffects.ApplyCrosshairRecoil(
					Crosshair,
					FirearmStats.HorizontalBaseRecoil,
					FirearmStats.HorizontalRecoilMin,
					FirearmStats.HorizontalRecoilRandom,
					FirearmStats.VerticalBaseRecoil,
					FirearmStats.VerticalRecoilMin,
					FirearmStats.VerticalRecoilRandom,
					FirearmStats.RecoilScale,
					FirearmStats.RecoilAccumilationScale,
					FirearmStats.HorizontalRecoilPunish
				);
			}
		};
	}

	private void InitializeFireGroupSettings()
	{
		_fireGroup.ResetState();
		_reloadBehaviour.ResetState();

		_fireGroup.OnFire += () =>
		{
			if (_reloadBehaviour.IsReloading && MagazineCount > 0 && _reloadBehaviour is { IsInterrupted: false } seq)
				seq.TryInterrupt();
			Attack();
		};
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

		_reloadBehaviour.OnReloadEnd += () =>
		{
			MagazineCount = FirearmStats.MagazineCapacity;
		};

		_reloadBehaviour.OnReloadProgress += (_, _, step) =>
		{
			MagazineCount += step;
		};

		_reloadBehaviour.TimeBetweenRounds = () =>
			(FirearmStats.ReloadTime * PlayerStats.ReloadTimeScale) / FirearmStats.MagazineCapacity;
		_reloadBehaviour.RoundsToLoad = FirearmStats.MagazineCapacity;
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

	private void Attack()
	{
		if (MagazineCount <= 0)
			return;

		if (_reloadBehaviour.IsReloading)
			return;

		if (Crosshair is null)
		{
			Logger.LogError("No crosshair");
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
		var mouseVector = Crosshair?.GlobalSpacePosition ?? Player.GetGlobalMousePosition();

		var bloomRad = FirearmStats.BloomCoefficientDeg * (Math.PI / 180);
		var bloom = (float)GD.RandRange(-bloomRad / 2, bloomRad / 2);

		var force = Vector2.Right.Rotated(nade.GlobalPosition.AngleToPoint(mouseVector) + bloom) * _throwForce;
		nade.LinearVelocity = force;
		GetTree().Root.Call(Window.MethodName.AddChild, nade);
		EmitSignalOnAttack();
	}

	public void Reload()
	{
		if (IsReloading)
			return;
		if (MagazineCount >= MagazineCapacity)
			return;
		_reloadBehaviour.RoundsToLoad = MagazineCapacity - MagazineCount;
		_reloadBehaviour.Reload();
	}

	protected override void HandleDamageECS(Entity entity)
	{
		ref var pos = ref GameWorld.World.Get<PositionComponent>(entity);

		var offset = (_lastBlastPosition.DistanceTo(pos.Position) / OffensiveStats.ProjectileRadius);
		offset = Mathf.Clamp(offset, 0, 1);
		var dropoffScale = _damageDropoffFromCenter.Sample(offset);

		OffensiveEffects.ApplyDamage(
			entity,
			Mathf.CeilToInt(OffensiveStats.Damage * dropoffScale),
			CalculateCrit(),
			OffensiveStats.DamageVarianceMultiplier,
			PlayerStats.OutgoingDamage
		);
	}

	protected override void HandleHitECS(Entity entity)
	{
		OffensiveEffects.ApplyVelocityMultiplier(entity, 0f);

		var health = GameWorld.World.Get<HealthComponent>(entity);
		if (health.Health > 0)
			return;
		GameWorld.World.Add(entity, new DeathCauseComponent(DeathCauseEnum.Explosion));
	}
}