using Game.Core.ECS;
using Game.Items.Offensive;
using Game.Levels.Controllers;
using Godot.Collections;

namespace Game.Levels.Environment;

public partial class Car : Node2D, IDestructable
{
	[Export]
	private int _health = 100;

	[Export]
	private int _explosionRadius = 512;

	[ExportGroup("Internal")]
	[Export]
	private PackedScene _explosionScene = null!;

	[Export]
	private PhysicsBody2D _body = null!;

	[Export]
	private Polygon2D _debugPolygon = null!;

	private bool _dead;

	public void TakeHit(int damage)
	{
		if (_dead)
			return;
		_health -= damage;
		EnvironmentFxManager.PlaySfx("car_hit");
		if (_health <= 0)
			Explode();
	}

	private void Explode()
	{
		_dead = true;
		var targetQuery = EnemyTargetQuery.Instance;
		if (targetQuery.TryGetTargetsInArea(GlobalPosition, _explosionRadius, out var targets))
		{
			foreach (var entity in targets)
			{
				if (!GameWorld.World.IsAlive(entity))
					continue;
				OffensiveEffects.ApplyDamage(entity, 100, 0, 0, 1);
				OffensiveEffects.ApplyKnockback(entity, GlobalPosition, 10);
			}
		}

		var data = new Dictionary<StringName, Variant>() { { "position", Position } };
		EnvironmentFxManager.PlaySfx("car_explosion", data);
		EnvironmentFxManager.PlayVfx("car_explosion", data);

		_debugPolygon.Color = Colors.DimGray;
		_body.ProcessMode = ProcessModeEnum.Disabled;
	}
}
