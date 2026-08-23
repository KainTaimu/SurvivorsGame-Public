using Game.Items.Offensive;
using Game.Levels.Controllers;
using Godot.Collections;

namespace Game.Levels.Environment;

public partial class Car : Node2D, IDestructable
{
	[Export]
	private int _health = 100;

	[ExportGroup("Internal")]
	[Export]
	private PackedScene _explosionScene = null!;

	public void TakeHit(int damage)
	{
		_health -= damage;
		EnvironmentFxManager.PlaySfx("car_hit");
		if (_health <= 0)
			Explode();
	}

	private void Explode()
	{
		var targetQuery = EnemyTargetQuery.Instance;
		if (targetQuery.TryGetTargetsInArea(GlobalPosition, 256, out var targets))
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
		EnvironmentFxManager.PlayVfx("car_explosion", data);

		QueueFree();
	}
}
