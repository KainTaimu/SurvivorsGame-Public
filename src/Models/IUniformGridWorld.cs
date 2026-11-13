// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace Game.Models;

/// <summary>
/// Represents a uniform grid where items of type <typeparamref name="T"/>
/// can be stored, retrieved, and managed based on world coordinates.
/// </summary>
/// <typeparam name="T">The type of items being stored in the grid world.</typeparam>
public interface IUniformGridWorld<T> : IUniformGrid<T, GridCellHandle>
{
	Vector2 Center { get; }
	Vector2I Dimensions { get; }
	int CellSize { get; }

	void Recenter(Vector2 position);

	bool TryGetWorld(Vector2 position, out T result, out GridCellHandle handle);
	GridCellHandle AddWorld(Vector2 position, T obj);
	void ClearCellWorld(Vector2 position);
	bool ContainsWorld(Vector2 position);
}
