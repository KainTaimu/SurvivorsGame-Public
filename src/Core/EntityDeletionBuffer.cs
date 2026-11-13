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
