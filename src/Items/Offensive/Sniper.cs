using Arch.Core;
using Game.Levels.Controllers;
using Game.Players.Controllers;
using Game.UI;
using Game.VFX;

namespace Game.Items.Offensive;

public partial class Sniper
	: BaseOffensive,
		IReloadable,
		IHasDamageStats,
		IHasMagazineStats,
		IHasBloomStats,
		IHasRecoilStats,
		IHasProjectileStats,
		IHasAttackStats,
		IHasPierceStats,
		IHasMoveBloomStats,
		IHasKnockbackStats
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
	public MoveBloomStats MoveBloomStats { get; set; } = null!;

	[Export]
	public KnockbackStats KnockbackStats { get; set; } = null!;

	[ExportCategory("")]
	[Export]
	private ScopeComponent _scopeComponent = null!;

	[ExportCategory("")]
	[Export]
	private PackedScene _projectileScene = null!;

	[Export]
	private AbstractFireGroup _fireGroup = null!;

	[Export]
	private AbstractReloadBehaviour _reloadBehaviour = null!;

	[Export]
	private AbstractProjectileAttack _projectileAttack = null!;

	[Export]
	private AudioStreamPlayer? _boltCyclingPlayer;

	private PlayerMovementController MovementController => GameWorld.Instance.MainPlayer.MovementController;

	public int MagazineCapacity => MagazineStats.Capacity;

	public int MagazineCount
	{
		get => MagazineStats.Count;
		set => MagazineStats.Count = value;
	}

	public bool IsReloading { get; private set; }

	private readonly ProjectilePool _pool = new();

	private static Crosshair? Crosshair => Crosshair.Instance;

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

		OnAttack += () => OffensiveEffects.ApplyCameraShake(RecoilStats.CameraRecoilScale, GetViewport, CreateTween);
		OnAttack += () =>
		{
			if (MagazineCount != 0)
			{
				GetTree().CreateTimer(AttackStats.AttackSpeed / 6, false).Timeout += () =>
				{
					_boltCyclingPlayer?.Play();
				};
			}

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
					RecoilStats.RecoilAccumilationScale
				);
			}
		};
	}

	public override void Process(double delta)
	{
		UpdateMoveTimeBloom(delta);

		(_fireGroup as ICooldown)?.Process((float)delta);
		_reloadBehaviour.Process((float)delta);

		if (Input.IsActionPressed(InputMapNames.WeaponReload))
		{
			Reload();
			return;
		}

		if (IsReloading)
			return;

		_fireGroup.ProcessInput();
		_scopeComponent.Process();
	}

	private void Attack()
	{
		DamageStats.CritDamageChance = (float)(1 - MoveBloomStats.MoveTimeFactor);
		DamageStats.Damage = (int)
			Math.Clamp(
				MoveBloomStats.MoveDamageMax * (1 - MoveBloomStats.MoveTimeFactor),
				MoveBloomStats.MoveDamageMin,
				MoveBloomStats.MoveDamageMax
			);

		if (_scopeComponent.ScopedIn)
			AttackScoped();
		else
			AttackNotScoped();
	}

	private void AttackNotScoped()
	{
		var bloomDeg = MoveBloomStats.MoveBloomMaxDeg * MoveBloomStats.MoveTimeFactor;
		bloomDeg = Math.Clamp(bloomDeg, MoveBloomStats.MoveBloomMinDeg, MoveBloomStats.MoveBloomMaxDeg);

		MagazineCount--;
		if (MagazineCount == 0)
			Reload();

		var playerPosition = Player.GetCanvasTransform() * Player.Position;

		var mouseVector = Crosshair?.CanvasSpacePosition ?? Player.GetGlobalMousePosition();
		var rotation = playerPosition.AngleToPoint(mouseVector);

		var bloomRad = bloomDeg * (Math.PI / 180);
		var bloom = (float)GD.RandRange(-bloomRad / 2, bloomRad / 2);
		rotation += bloom;
		var scale = Vector2.One * ProjectileStats.Scale;

		_projectileAttack.Attack(
			_pool.GetProjectile,
			this,
			Player.GlobalPosition,
			rotation,
			ProjectileStats.HitRadius,
			ProjectileStats.Speed,
			PierceStats.PierceLimit,
			scale
		);

		EmitSignalOnAttack();
	}

	private void AttackScoped()
	{
		MagazineCount--;
		if (MagazineCount == 0)
			Reload();

		var mousePos = Crosshair?.GlobalSpacePosition ?? Player.GetGlobalMousePosition();

		if (TargetQuery.TryGetTargetsInArea(mousePos, ProjectileStats.HitRadius, out var targetIds))
		{
			foreach (var entity in targetIds)
				TryHandleHit(entity);
		}
		EmitSignalOnAttack();
	}

	public void Reload()
	{
		if (IsReloading)
			return;
		if (MagazineCount >= MagazineCapacity)
			return;
		GetTree().CreateTimer(ReloadStats.ReloadTime, false).Timeout += () =>
		{
			MagazineCount = MagazineCapacity;
			IsReloading = false;
			EmitSignalOnReloadEnd();
		};
		IsReloading = true;
		EmitSignalOnReloadStart();
	}

	private void UpdateMoveTimeBloom(double delta)
	{
		if (MovementController.Velocity.LengthSquared() > 0 || Crosshair?.CrosshairVelocity.LengthSquared() > 0)
			MoveBloomStats.MoveTime += delta * MoveBloomStats.MoveBloomGrowthRate;
		else
			MoveBloomStats.MoveTime -= delta * MoveBloomStats.MoveBloomShrinkRate;

		SpreadCrosshair((float)MoveBloomStats.MoveTimeFactor);
	}

	private void SpreadCrosshair(float spreadRatio)
	{
		Crosshair?.ChangePrimaryCrosshairSpread(spreadRatio);
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
	}
}
