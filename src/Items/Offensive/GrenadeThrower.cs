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
	public float ThrowForce = 100;

	public string? AttackActionString { get; set; }

	private Crosshair? Crosshair => Crosshair.Instance;
	private double _fireCooldown;

	private ProjectilePool _projectilePool = null!;

	private Vector2 _blastPosition;

	public override void _Ready()
	{
		_projectilePool = new ProjectilePool { ProjectileScene = GrenadeScene };
		AddChild(_projectilePool);
	}

	public override void _Process(double delta)
	{
		_fireCooldown -= delta;

		if (!Input.IsActionPressed(AttackActionString ?? InputMapNames.PrimaryAttack))
			return;
		Attack();
	}

	public override void Attack()
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
		nade.OnExploded += (pos) => _blastPosition = pos;

		nade.OffensiveOrigin = this;
		nade.GlobalPosition = Player.GlobalPosition;
		var force =
			new Vector2(Mathf.Cos(Crosshair.AngleFromPlayer), Mathf.Sin(Crosshair.AngleFromPlayer)) * ThrowForce
			+ Player.MovementController.Velocity;
		nade.ApplyImpulse(force);
		GetTree().Root.CallDeferred(Window.MethodName.AddChild, nade);
		GetTree().CreateTimer(OffensiveStats.AttackSpeed * 0.5).Timeout += QueueFree;
	}

	protected override void HandleHitECS(Entity entity)
	{
		base.HandleHitECS(entity);
		if (GameWorld.World.Has<PositionComponent>(entity))
		{
			ref var pos = ref GameWorld.World.Get<PositionComponent>(entity);
			var knockback = OffensiveStats.Additional.GetValueOrDefault("Knockback").AsSingle();
			var knockbackVector = _blastPosition.DirectionTo(pos.Position);
			knockbackVector *= knockback;
			pos.Position += knockbackVector;
		}

		if (GameWorld.World.Has<VelocityComponent>(entity))
		{
			ref var vel = ref GameWorld.World.Get<VelocityComponent>(entity);
			vel.Velocity = Vector2.Zero;
		}

		if (!GameWorld.World.TryGet<HealthComponent>(entity, out var health))
			return;
		if (health.Health > 0)
			return;

		GameWorld.World.Add(entity, new DeathCauseComponent(DeathCauseEnum.Explosion));
	}
}
