using System.Collections.Generic;
using Arch.Core;
using Game.Levels.Controllers;
using Game.Players.Controllers;
using Game.UI;

namespace Game.Items.Offensive;

public partial class Minigun
	: BaseOffensive,
		IReloadable,
		IHasDamageStats,
		IHasMagazineStats,
		IHasBloomStats,
		IHasRecoilStats,
		IHasProjectileStats,
		IHasAttackStats,
		IHasPierceStats
{
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
	public PushOnAttackStats PushOnAttackStats { get; set; } = null!;

	[Export]
	public KnockbackStats KnockbackStats { get; set; } = null!;

	[ExportCategory("")]
	[Export]
	private PackedScene _projectileScene = null!;

	[Export]
	private AbstractProjectileAttack _projectileAttack = null!;

	[Export]
	private StatusEffect? _movementPenaltyStatusEffect;

	[Export]
	private StatusEffect? _movementPenaltyWearOffStatusEffect;

	private bool _isMovementPenaltyApplied;
	private bool _isMovementPenaltyWearOffApplied;

	public int MagazineCapacity => MagazineStats.Capacity;
	public int MagazineCount { get; set; }
	public bool IsReloading => false;

	private readonly ProjectilePool _pool = new();

	private static Crosshair? Crosshair => Crosshair.Instance;

	[Export]
	private Curve _windupCurve = null!;

	private double _windupTime;

	private float WindupAttackSpeed =>
		Math.Clamp(_windupCurve.Sample((float)_windupTime), AttackStats.AttackSpeed, _windupCurve.MaxDomain);

	private float _fireCooldown;

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

		OnEquipped += () =>
		{
			if (_movementPenaltyStatusEffect is null)
				return;
			if (_isMovementPenaltyApplied)
				return;

			Player.StatusEffectController.AddStatusEffect(_movementPenaltyStatusEffect);
			_isMovementPenaltyApplied = true;

			if (_movementPenaltyWearOffStatusEffect is not null && _isMovementPenaltyWearOffApplied)
			{
				Player.StatusEffectController.RemoveStatusEffect(_movementPenaltyWearOffStatusEffect);
				_isMovementPenaltyWearOffApplied = false;
			}
		};
		OnUnequipped += () =>
		{
			if (_movementPenaltyStatusEffect is null)
				return;
			if (!_isMovementPenaltyApplied)
				return;

			Player.StatusEffectController.RemoveStatusEffect(_movementPenaltyStatusEffect);
			_isMovementPenaltyApplied = false;

			if (_movementPenaltyWearOffStatusEffect is not null && !_isMovementPenaltyWearOffApplied)
			{
				Player.StatusEffectController.AddStatusEffect(_movementPenaltyWearOffStatusEffect);
				_isMovementPenaltyWearOffApplied = true;
			}
		};
	}

	public override void Process(double delta)
	{
		if (Input.IsActionPressed(InputMapNames.PrimaryAttack))
		{
			_fireCooldown -= (float)delta;
			_windupTime = Math.Clamp(_windupTime + delta, 0, _windupCurve.MaxDomain);
			Attack();
		}
		else
			_windupTime = Math.Clamp(_windupTime - delta * 3, 0, _windupCurve.MaxDomain);
	}

	public void Attack()
	{
		if (_fireCooldown > 0)
			return;

		MagazineStats.Count--;
		if (MagazineStats.Count == 0)
			Reload();

		var playerPosition = Player.GetCanvasTransform() * Player.Position;

		var mouseVector = Crosshair?.CanvasSpacePosition ?? Player.GetGlobalMousePosition();
		var rotation = playerPosition.AngleToPoint(mouseVector);

		var bloomRad = BloomStats.BloomCoefficientDeg * (Math.PI / 180);
		var bloom = (float)GD.RandRange(-bloomRad / 2, bloomRad / 2);
		rotation += bloom;

		var scale = Vector2.One * ProjectileStats.Scale;

		_projectileAttack.Attack(
			_pool.GetProjectile,
			Player.GlobalPosition,
			rotation,
			ProjectileStats.HitRadius,
			ProjectileStats.Speed,
			PierceStats.PierceLimit,
			scale
		);

		PushPlayer(rotation, bloom);

		_fireCooldown = WindupAttackSpeed;
		EmitSignalOnAttack();
	}

	public void Reload()
	{
		QueueFree();
	}

	private void PushPlayer(float rotation, float bloom)
	{
		Player.GlobalPosition +=
			Vector2.Right.Rotated(rotation - Mathf.Pi + bloom)
			* PushOnAttackStats.PushPerAttack
			* (float)GetProcessDeltaTime();
	}

	protected override void HandleDamage(Entity entity)
	{
		OffensiveEffects.ApplyDamage(
			entity,
			Mathf.CeilToInt(DamageStats.Damage),
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
