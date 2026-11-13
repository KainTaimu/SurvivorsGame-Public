// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

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
