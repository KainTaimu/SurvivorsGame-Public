// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

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

	public float TimeToExplode { get; set; }

	public int EnemiesInAreaUntilExplode { get; set; }

	public float ArmingDistance { get; set; }

	public float CameraRecoilScale { get; set; }

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
			((IHasProjectileStats)OffensiveOrigin).ProjectileStats.HitRadius,
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
				((IHasProjectileStats)OffensiveOrigin).ProjectileStats.HitRadius,
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
				continue;

			OffensiveOrigin.HandleHit(entity);

			if (OffensiveOrigin is IHasKnockbackStats knockbackStats)
				OffensiveEffects.ApplyKnockback(entity, GlobalPosition, knockbackStats.KnockbackStats.KnockbackForce);
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
