using System.Collections.Generic;
using Arch.Core;

namespace Game.Levels.Controllers;

[GlobalClass]
public partial class EnemyTracker : Node
{
	public static int EnemyCount => _entities.Count;

	private static readonly HashSet<Entity> _entities = [];

	public override void _EnterTree()
	{
		GameWorld.World.SubscribeEntityDestroyed(
			(in e) =>
			{
				if (!_entities.Remove(e))
					throw new Exception("Attempt to destroy a non-existant entity");
			}
		);
	}

	public override void _ExitTree()
	{
		_entities.Clear();
	}

	public static Entity CreateEntity()
	{
		var entity = GameWorld.World.Create();
		_entities.Add(entity);

		return entity;
	}
}