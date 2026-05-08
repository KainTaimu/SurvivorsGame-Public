using System.Collections.Generic;
using System.Linq;
using Game.Core.ECS;
using Game.Levels.Controllers;

namespace Game.Items.Offensive;

public partial class Grenade : RigidBody2D
{
	public BaseOffensive OffensiveOrigin = null!;

	private EnemyTargetQuery TargetQuery => EnemyTargetQuery.Instance;
	private EntityComponentStore ComponentStore =>
		EntityComponentStore.Instance;

	private readonly HashSet<int> _hits = [];
	private double _t = 0.5;

	public override void _Process(double delta)
	{
		_t -= delta;

		TargetQuery.TryGetTargetsInArea(
			Position,
			OffensiveOrigin.Stats.ProjectileRadius,
			out var ids
		);

		if (ids.Count() > 6 && _t < 0.1f)
		{
			foreach (var id in ids)
			{
				OffensiveOrigin.HandleHit(id: id);
			}
			QueueFree();
		}

		if (_t <= 0)
		{
			Logger.LogDebug(string.Join(", ", ids));

			foreach (var id in ids)
			{
				OffensiveOrigin.HandleHit(id: id);
			}
			QueueFree();
		}
	}
}
