// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace Game.Models;

public interface IUniformGrid<T, THandle>
	where THandle : struct, IItemHandle
{
	int Count { get; }

	void EnsureCapacity(int newCapacity);
	bool TryGet(int x, int y, out T result, out THandle handle);
	bool TryGet(THandle handle, out T result);
	THandle Add(int x, int y, T obj);
	void Remove(THandle targetHandle);
	void ClearAll();
	void ClearCell(int x, int y);
	int GetCellCount(int x, int y);
	bool IsValidCell(int x, int y);
	CellEnumerator<T> GetEnumerator(int x, int y);
}
