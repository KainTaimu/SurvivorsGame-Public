using System.Collections.Concurrent;
using Arch.Buffer;
using Game.Levels.Controllers;

namespace Game.Core;

[GlobalClass]
public partial class EntityCommandBuffer : Node, IEntityCommandBuffer
{
	public static IEntityCommandBuffer Instance = null!;

	private readonly ConcurrentQueue<CommandBuffer> _buffers = [];

	public override void _Ready()
	{
		if (ProcessPriority != int.MaxValue)
		{
			Logger.LogWarning($"EntityCommandBuffer node \"{Name}\" should have ProcessPriority = {int.MaxValue}!");
		}
		Instance = this;
	}

	public override void _ExitTree()
	{
		Instance = this;
	}

	public override void _Process(double delta)
	{
		CallDeferred(MethodName.ProcessCommands);
	}

	private void ProcessCommands()
	{
		if (_buffers.IsEmpty)
			return;
		while (_buffers.TryDequeue(out var buffer))
		{
			buffer.Playback(GameWorld.World);
		}
	}

	public void PushCommand(CommandBuffer buffer)
	{
		_buffers.Enqueue(buffer);
	}
}
