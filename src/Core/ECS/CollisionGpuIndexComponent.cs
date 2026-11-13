namespace Game.Core.ECS;

// Dense slot (0..N-1) written by the GPU collision solver's upload pass
// and read by its apply pass. -1 = not participating this frame.
public record struct CollisionGpuIndexComponent(int Index)
{
	public static readonly CollisionGpuIndexComponent NotParticipating = new(-1);
}
