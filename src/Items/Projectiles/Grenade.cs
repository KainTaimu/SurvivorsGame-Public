using System.Collections.Generic;
using Arch.Core;
using Game.Core.Settings;
using Game.Levels.Controllers;

namespace Game.Items.Offensive;

public partial class Grenade : RigidBody2D
{
	[Signal]
	public delegate void OnExplodedEventHandler(Vector2 blastPosition);

	[Export]
	private PackedScene _explosionScene = null!;

	[Export]
	private PackedScene _postExplosionFireScene = null!;

	public BaseOffensive OffensiveOrigin = null!;

	private EnemyTargetQuery TargetQuery => EnemyTargetQuery.Instance;

	private double _t;
	private double _distanceTraveled;

	private float TimeToExplode => OffensiveOrigin.Stats.Additional.GetValueOrDefault("TimeToExplode").AsSingle();

	private int EnemiesInAreaUntilExplode =>
		OffensiveOrigin.Stats.Additional.GetValueOrDefault("EnemiesInAreaUntilExplode", 6).AsInt32();

	private float ArmingDistance =>
		OffensiveOrigin.Stats.Additional.GetValueOrDefault("ArmingDistance", 200f).AsSingle();

	private float CameraRecoilScale =>
		OffensiveOrigin.Stats.Additional.GetValueOrDefault("CameraRecoilScale").AsSingle();

	public override void _Ready()
	{
		_t = TimeToExplode;
	}

	public override void _ExitTree()
	{
		var explosion = _explosionScene.Instantiate<GpuParticles2D>();
		explosion.Emitting = true;
		explosion.GlobalPosition = GlobalPosition;
		GetTree().Root.CallDeferred(Window.MethodName.AddChild, explosion);

		var fire = _postExplosionFireScene.Instantiate<GpuParticles2D>();
		fire.Emitting = true;
		fire.GlobalPosition = GlobalPosition;
		GetTree().Root.CallDeferred(Window.MethodName.AddChild, fire);
	}

	public override void _PhysicsProcess(double delta)
	{
		_t -= delta;
		_distanceTraveled += LinearVelocity.Length() * delta;

		Rotation = LinearVelocity.Angle();

		if (Engine.GetPhysicsFrames() % 5 != 0)
			return;

		TargetQuery.TryGetTargetsInArea(
			GlobalPosition,
			OffensiveOrigin.OffensiveStats.ProjectileRadius,
			out var entities
		);

		if (entities.Length > EnemiesInAreaUntilExplode && _distanceTraveled > ArmingDistance)
		{
			ExplodeWithDelay();
			return;
		}

		if (_t <= 0)
			Explode(entities);
	}

	private void ExplodeWithDelay()
	{
		GetTree().CreateTimer(0.1f, false).Timeout += () =>
		{
			TargetQuery.TryGetTargetsInArea(
				GlobalPosition,
				OffensiveOrigin.OffensiveStats.ProjectileRadius,
				out var entities
			);
			Explode(entities);
		};
	}

	private void Explode(Entity[] entitiesHit)
	{
		EmitSignalOnExploded(GlobalPosition);

		foreach (var entity in entitiesHit)
		{
			if (!GameWorld.World.IsAlive(entity))
				return;

			OffensiveOrigin.HandleHit(entity);

			OffensiveEffects.ApplyKnockback(
				entity,
				GlobalPosition,
				OffensiveOrigin.OffensiveStats.Additional.GetValueOrDefault("Knockback", 0f).AsSingle()
			);
		}

		ApplyCameraRecoil();
		QueueFree();
	}

	private void ApplyCameraRecoil()
	{
		if (!GameSettings.Instance.EnableCameraShake)
			return;
		if (CameraRecoilScale == 0)
			return;

		var camera = GetViewport().GetCamera2D();

		var origPos = camera.Position;
		var tween = GetTree().CreateTween().SetTrans(Tween.TransitionType.Spring);

		for (var i = 0; i < 6; i++)
		{
			static int Rand()
			{
				return GD.RandRange(-1, 1);
			}

			var shake = new Vector2(Rand(), Rand()) * GD.RandRange(4, 9) * CameraRecoilScale;

			tween.TweenProperty(camera, "offset", camera.Position + shake, 1 / 30f);
		}

		tween.TweenProperty(camera, "offset", origPos, 1 / 8f);
	}
}
