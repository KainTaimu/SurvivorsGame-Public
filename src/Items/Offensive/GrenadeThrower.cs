using System.Collections.Generic;
using Arch.Core;
using Game.Core.ECS;
using Game.Levels.Controllers;
using Game.UI;

namespace Game.Items.Offensive;

public partial class GrenadeThrower : BaseOffensive, IManualAttack
{
	[Export]
	public PackedScene GrenadeScene = null!;

	[Export]
	private AudioStreamPlayer? _explosionPlayer;

	[Export]
	public float ThrowForce = 1250;

	public string? AttackActionString { get; set; }

	private Crosshair? Crosshair => Crosshair.Instance;
	private double _fireCooldown;

	private Vector2 _blastPosition;

	public override void _Process(double delta)
	{
		_fireCooldown -= delta;

		if (!Input.IsActionPressed(AttackActionString ?? InputMapNames.PrimaryAttack))
			return;
		Attack();
	}

	public void Attack()
	{
		if (Crosshair is null)
		{
			Logger.LogError("No crosshair");
			return;
		}

		if (_fireCooldown > 0)
			return;
		_fireCooldown = OffensiveStats.AttackSpeed;

		var nade = GrenadeScene.Instantiate<Grenade>();
		nade.OnExploded += (pos) =>
		{
			_blastPosition = pos;
			_explosionPlayer?.Reparent(GetTree().Root);
			_explosionPlayer?.Play();
			Callable.From(QueueFree).CallDeferred();
		};

		nade.OffensiveOrigin = this;
		nade.GlobalPosition = Player.GlobalPosition;
		var force =
			Vector2.Right.Rotated(
				GetViewport().GetCamera2D().GetScreenCenterPosition().AngleToPoint(Crosshair.GlobalSpacePosition)
			) * ThrowForce
			+ Player.MovementController.Velocity;
		nade.ApplyImpulse(force);
		GetTree().Root.CallDeferred(Window.MethodName.AddChild, nade);
	}

	protected override void HandleHitECS(Entity entity)
	{
		OffensiveEffects.ApplyDamage(
			entity,
			OffensiveStats.Damage,
			CalculateCrit(),
			OffensiveStats.DamageVarianceMultiplier,
			PlayerStats.OutgoingDamageMultiplier
		);

		OffensiveEffects.ApplyVelocityMultiplier(entity, 0f);

		if (!GameWorld.World.TryGet<HealthComponent>(entity, out var health))
			return;
		if (health.Health > 0)
			return;
		GameWorld.World.Add(entity, new DeathCauseComponent(DeathCauseEnum.Explosion));
	}
}
