// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Arch.Core;
using Game.Core.ECS;
using Game.Levels.Controllers;
using Game.UI;

namespace Game.Items.Offensive;

public partial class GrenadeThrower
	: BaseOffensive,
		IHasGrenadeStats,
		IHasAttackStats,
		IHasProjectileStats,
		IHasRecoilStats
{
	[ExportCategory("Stats")]
	[Export]
	public DamageStats DamageStats { get; set; } = null!;

	[Export]
	public ProjectileStats ProjectileStats { get; set; } = null!;

	[Export]
	public AttackStats AttackStats { get; set; } = null!;

	[Export]
	public GrenadeStats GrenadeStats { get; set; } = null!;

	[Export]
	public RecoilStats RecoilStats { get; set; } = null!;

	[ExportCategory("")]
	[Export]
	public PackedScene GrenadeScene = null!;

	[Export]
	private AudioStreamPlayer? _explosionPlayer;

	[Export]
	public float ThrowForce = 1250;

	[Export]
	private Curve _damageDropoffFromCenter = null!;

	private Crosshair? Crosshair => Crosshair.Instance;
	private double _fireCooldown;

	private Vector2 _blastPosition;

	public override void Process(double delta)
	{
		_fireCooldown -= delta;

		if (!Input.IsActionPressed(InputMapNames.PrimaryAttack))
			return;
		Attack();
	}

	public void Attack()
	{
		if (Crosshair is null)
		{
			CustomLogger.LogError("No crosshair");
			return;
		}

		if (_fireCooldown > 0)
			return;
		_fireCooldown = AttackStats.AttackSpeed;

		var nade = GrenadeScene.Instantiate<Grenade>();
		nade.OnExploded += pos =>
		{
			_blastPosition = pos;
			_explosionPlayer?.Reparent(GetTree().Root);
			_explosionPlayer?.Play();
			Callable.From(QueueFree).CallDeferred();
		};

		nade.OffensiveOrigin = this;
		nade.GlobalPosition = Player.GlobalPosition;
		nade.GlobalPosition = Player.GlobalPosition;
		nade.ArmingDistance = GrenadeStats.ArmingDistance;
		nade.CameraRecoilScale = RecoilStats.CameraRecoilScale;
		nade.EnemiesInAreaUntilExplode = GrenadeStats.EnemiesInAreaUntilExplode;
		nade.TimeToExplode = GrenadeStats.TimeToExplode;

		var force =
			Vector2.Right.Rotated(
				GetViewport().GetCamera2D().GetScreenCenterPosition().AngleToPoint(Crosshair.GlobalSpacePosition)
			) * ThrowForce
			+ Player.MovementController.Velocity;
		nade.LinearVelocity = force;
		GetTree().Root.CallDeferred(Window.MethodName.AddChild, nade);
	}

	protected override void HandleDamage(Entity entity)
	{
		ref var pos = ref GameWorld.World.Get<PositionComponent>(entity);

		var offset = _blastPosition.DistanceTo(pos.Position) / ProjectileStats.HitRadius;
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
