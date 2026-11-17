// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

using Arch.Core;
using CommunityToolkit.HighPerformance.Buffers;
using Game.Core;
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
	public AbstractFireGroup FireGroup { get; private set; } = null!;

	[Export]
	public AbstractReloadBehaviour ReloadBehaviour { get; private set; } = null!;

	[Export]
	public AbstractProjectileAttack ProjectileAttack { get; private set; } = null!;

	[Export]
	private AudioStreamPlayer? _boltCyclingPlayer;

	private PlayerMovementController MovementController => GameWorld.Instance.MainPlayer.MovementController;

	public int MagazineCapacity => MagazineStats.Capacity;

	public int MagazineCount
	{
		get => MagazineStats.Count;
		set => MagazineStats.Count = value;
	}

	public bool IsReloading => ReloadBehaviour.IsReloading;
	public ReloadDisplayType ReloadDisplayType => ReloadBehaviour.DisplayType;

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
		(FireGroup as ICooldown)?.Process((float)delta);
		ReloadBehaviour.Process((float)delta);

		if (Input.IsActionPressed(InputMapNames.WeaponReload))
		{
			Reload();
			return;
		}

		if (IsReloading)
			return;

		FireGroup.ProcessInput();
		_scopeComponent.Process();
	}

	private void Attack()
	{
		if (_scopeComponent.ScopedIn)
			AttackScoped();
		else
			AttackNotScoped();
	}

	private void AttackNotScoped()
	{
		MagazineCount--;
		if (MagazineCount == 0)
			Reload();

		var playerPosition = Player.GetCanvasTransform() * Player.Position;

		var mouseVector = Crosshair?.CanvasSpacePosition ?? Player.GetGlobalMousePosition();
		var rotation = playerPosition.AngleToPoint(mouseVector);

		var bloomRad = BloomStats.BloomCoefficientDeg * (Math.PI / 180);
		var bloom = (float)GD.RandRange(-bloomRad / 2, bloomRad / 2);
		rotation += bloom;
		var scale = Vector2.One * ProjectileStats.Scale;

		ProjectileAttack.Attack(
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

		const int maxHitBufferSize = Globals.MAX_STACKALLOC_COUNT / 12;
		if (PierceStats.PierceLimit <= maxHitBufferSize)
		{
			Span<Entity> hitBuffer = stackalloc Entity[PierceStats.PierceLimit];
			var hitCount = TargetQuery.TryGetTargetsInArea(
				mousePos,
				ProjectileStats.HitRadius,
				hitBuffer,
				PierceStats.PierceLimit
			);

			for (var i = 0; i < hitCount; i++)
			{
				var entity = hitBuffer[i];
				TryHandleHit(entity);
			}
		}
		else
		{
			using var hitBuffer = SpanOwner<Entity>.Allocate(PierceStats.PierceLimit);
			var hitCount = TargetQuery.TryGetTargetsInArea(
				mousePos,
				ProjectileStats.HitRadius,
				hitBuffer.Span,
				PierceStats.PierceLimit
			);

			for (var i = 0; i < hitCount; i++)
			{
				var entity = hitBuffer.Span[i];
				TryHandleHit(entity);
			}
		}

		EmitSignalOnAttack();
	}

	public void Reload()
	{
		ReloadBehaviour.Reload();
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
