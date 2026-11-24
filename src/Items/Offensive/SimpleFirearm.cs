// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

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
		IHasDamageStats,
		IHasMagazineStats,
		IHasBloomStats,
		IHasRecoilStats,
		IHasProjectileStats,
		IHasAttackStats,
		IMayHasPierceStats,
		IMayHasSlowStats,
		IMayHasKnockbackStats,
		IMayHasBurstFireStats
{
	// TODO: Remove these. AbstractReloadBehaviour's signals should be used instead.
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
	public PierceStats? PierceStats { get; set; }

	[Export]
	public SlowStats? SlowStats { get; set; }

	[Export]
	public KnockbackStats? KnockbackStats { get; set; }

	[Export]
	public BurstFireStats? BurstFireStats { get; set; }

	[ExportCategory("")]
	[Export]
	protected PackedScene ProjectileScene { get; private set; } = null!;

	[Export]
	protected AbstractFireGroup FireGroup { get; private set; } = null!;

	[Export]
	public AbstractReloadBehaviour ReloadBehaviour { get; private set; } = null!;

	[Export]
	protected AbstractProjectileAttack ProjectileAttack { get; private set; } = null!;

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

	public bool IsReloading => ReloadBehaviour.IsReloading;

	public ReloadDisplayType ReloadDisplayType => ReloadBehaviour.DisplayType;

	protected readonly ProjectilePool Pool = new();

	private static Crosshair? Crosshair => Crosshair.Instance;

	private bool _fireGroupRegistered;

	public override void _Ready()
	{
		Pool.Initialize(this, ProjectileScene, p =>
		{
			p.OnEntityHit += e => TryHandleHit(e.Entity);
		});

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

	public override void Process(double delta)
	{
		(FireGroup as ICooldown)?.Process((float)delta);
		ReloadBehaviour.Process((float)delta);

		if (Input.IsActionPressed(InputMapNames.WeaponReload))
		{
			Reload();
			return;
		}

		FireGroup.ProcessInput();
	}

	public virtual bool Attack()
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

		rotation += BloomStats.CalculateBloomRads();
		var scale = Vector2.One * ProjectileStats.Scale;

		ProjectileAttack.Attack(
			Pool.GetProjectile,
			this,
			Player.GlobalPosition,
			rotation,
			ProjectileStats.HitRadius,
			ProjectileStats.Speed,
			PierceStats?.PierceLimit ?? 1,
			scale
		);

		EmitSignalOnAttack();
		return true;
	}

	public virtual void Reload()
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
		if (KnockbackStats is not null)
			OffensiveEffects.ApplyKnockback(entity, Player.GlobalPosition, KnockbackStats.KnockbackForce);
		if (SlowStats is not null)
			OffensiveEffects.ApplyVelocityMultiplier(entity, SlowStats.SlowCoefficient);
	}
}
