// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

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
