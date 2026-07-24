using Arch.Buffer;

namespace Game.Core;

public interface IEntityCommandBuffer
{
	void PushCommand(CommandBuffer buffer);
}