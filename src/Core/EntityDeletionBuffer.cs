// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Concurrent;
using Arch.Core;
using Game.Levels.Controllers;

namespace Game.Core;

public class EntityDeletionBuffer
{
	private readonly ConcurrentQueue<Entity> _buffers = [];

	public void ProcessDeletionQueue()
	{
		if (_buffers.IsEmpty)
			return;
		while (_buffers.TryDequeue(out var entity))
			GameWorld.World.Destroy(entity);
	}

	public void PushDeletionCommand(Entity entity)
	{
		_buffers.Enqueue(entity);
	}
}
