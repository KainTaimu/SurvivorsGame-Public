using System.Runtime.CompilerServices;

namespace Game.Models;

public interface IItemHandle;

public readonly struct GridCellHandle : IItemHandle
{
	internal readonly int Index;
	internal readonly uint Generation;
	internal readonly uint Epoch;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal GridCellHandle(int index, uint generation, uint epoch)
	{
		Index = index;
		Generation = generation;
		Epoch = epoch;
	}
}
