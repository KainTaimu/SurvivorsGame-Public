using System.Runtime.CompilerServices;

namespace Game.Models;

public struct CellEnumerator<T>
{
	private readonly UniformGrid<T>.Entry[] _entries;
	private readonly int _sentinel;
	private int _current;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal CellEnumerator(UniformGrid<T>.Entry[] entries, int sentinel)
	{
		_entries = entries;
		_sentinel = sentinel;
		_current = sentinel;
	}

	public readonly T Current
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _entries[_current].Item;
	}

	public readonly ref T CurrentRef
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref _entries[_current].Item;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool MoveNext()
	{
		_current = _entries[_current].Next;
		return _current != _sentinel;
	}

	/// <summary>
	/// Returns a copy positioned so that the first MoveNext yields the item
	/// after the current one. Enables j = i + 1 style pair iteration.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly CellEnumerator<T> CloneRest()
	{
		return this;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly CellEnumerator<T> GetEnumerator()
	{
		return this;
	}
}