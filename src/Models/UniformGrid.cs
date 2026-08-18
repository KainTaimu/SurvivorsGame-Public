using System.Runtime.CompilerServices;

namespace Game.Models;

public class UniformGrid<T> : IUniformGrid<T, GridCellHandle>
{
	public int Count { get; private set; }

	public readonly int Width;
	public readonly int Height;

	private Entry[] _entries;
	private int _freeHead;
	private uint _epoch;

	public UniformGrid(int width, int height, int initialCapacity = 64)
	{
		Width = width;
		Height = height;
		_entries = new Entry[width * height + initialCapacity];
		ResetCells();
		LinkFreeRange(width * height);
	}

	public void EnsureCapacity(int newCapacity)
	{
		var oldLength = _entries.Length;
		if (newCapacity <= oldLength - Width * Height)
			return;

		Array.Resize(ref _entries, Width * Height + newCapacity);
		LinkFreeRange(oldLength);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGet(int x, int y, out T result, out GridCellHandle handle)
	{
		var sentinel = CellIndex(x, y);
		ref var head = ref _entries[sentinel];
		if (head.Next == sentinel)
		{
			result = default!;
			handle = default;
			return false;
		}

		ref var entry = ref _entries[head.Next];
		result = entry.Item;
		handle = new GridCellHandle(head.Next, entry.Generation, _epoch);
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGet(GridCellHandle handle, out T result)
	{
		if (!IsValidHandle(handle))
		{
			result = default!;
			return false;
		}

		result = _entries[handle.Index].Item;
		return true;
	}

	public GridCellHandle Add(int x, int y, T obj)
	{
		if (_freeHead == -1)
			EnsureCapacity((_entries.Length - Width * Height) * 2 + 1);

		var slot = _freeHead;
		ref var entry = ref _entries[slot];
		_freeHead = entry.Next;

		var sentinel = CellIndex(x, y);
		ref var head = ref _entries[sentinel];

		entry.Item = obj;
		entry.Next = head.Next;
		entry.Previous = sentinel;
		entry.Generation++;
		entry.Cell = sentinel;

		_entries[head.Next].Previous = slot;
		head.Next = slot;
		head.Count++;

		Count++;
		return new GridCellHandle(slot, entry.Generation, _epoch);
	}

	public void Remove(GridCellHandle targetHandle)
	{
		if (!IsValidHandle(targetHandle))
			return;

		ref var entry = ref _entries[targetHandle.Index];

		_entries[entry.Previous].Next = entry.Next;
		_entries[entry.Next].Previous = entry.Previous;
		_entries[entry.Cell].Count--;

		entry.Next = _freeHead;
		entry.Previous = -1;
		entry.Generation++;
		_freeHead = targetHandle.Index;

		Count--;
	}

	public void ClearAll()
	{
		var cellCount = Width * Height;
		_epoch++;
		ResetCells();
		_freeHead = -1;
		LinkFreeRange(cellCount);
		Count = 0;
	}

	public void ClearCell(int x, int y)
	{
		var sentinel = CellIndex(x, y);
		ref var head = ref _entries[sentinel];

		var i = head.Next;
		while (i != sentinel)
		{
			ref var entry = ref _entries[i];
			var next = entry.Next;
			entry.Next = _freeHead;
			entry.Previous = -1;
			entry.Generation++;
			_freeHead = i;
			Count--;
			i = next;
		}

		head.Next = sentinel;
		head.Previous = sentinel;
		head.Count = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetCellCount(int x, int y)
	{
		return _entries[CellIndex(x, y)].Count;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsValidCell(int x, int y)
	{
		return (uint)x < (uint)Width && (uint)y < (uint)Height;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public CellEnumerator<T> GetEnumerator(int x, int y)
	{
		return new CellEnumerator<T>(_entries, CellIndex(x, y));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool IsValidHandle(GridCellHandle handle)
	{
		return handle.Epoch == _epoch
			&& handle.Index >= Width * Height
			&& handle.Index < _entries.Length
			&& _entries[handle.Index].Generation == handle.Generation;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int CellIndex(int x, int y)
	{
		return y * Width + x;
	}

	private void ResetCells()
	{
		var cellCount = Width * Height;
		for (var i = 0; i < cellCount; i++)
		{
			ref var sentinel = ref _entries[i];
			sentinel.Next = i;
			sentinel.Previous = i;
			sentinel.Count = 0;
		}
	}

	private void LinkFreeRange(int start)
	{
		for (var i = start; i < _entries.Length; i++)
		{
			ref var entry = ref _entries[i];
			entry.Next = _freeHead;
			entry.Previous = -1;
			_freeHead = i;
		}
	}

	internal struct Entry()
	{
		public T Item = default!;
		public int Next = -1;
		public int Previous = -1;
		public int Cell = -1;
		public int Count = 0;
		public uint Generation = 0;
	}
}
