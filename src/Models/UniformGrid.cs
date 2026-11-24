// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Models;

public class UniformGrid<T>
	where T : struct
{
	public readonly UniformGridCell<T>[] Cells;

	public readonly Vector2I Dimensions;

	public readonly int CellSize;

	public UniformGrid(int cellSize, int sizeX, int sizeY)
	{
		if (cellSize <= 0)
		{
			CustomLogger.LogError("Invalid cell size, cellSize must be > 0. Setting as 16px.");
			cellSize = 16;
		}

		var width = sizeX / cellSize;
		var height = sizeY / cellSize;

		Cells = new UniformGridCell<T>[width * height];
		Dimensions = new Vector2I(width, height);
		CellSize = cellSize;
	}

	public int GetCellIndex(int x, int y)
	{
		if (x < 0 || y < 0 || x >= Dimensions.X || y >= Dimensions.Y)
			return -1;

		return y * Dimensions.X + x;
	}

	public int GetCellIndex(Vector2 position)
	{
		if (position.X < 0f || position.Y < 0f)
			return -1;

		return GetCellIndex((int)(position.X / CellSize), (int)(position.Y / CellSize));
	}

	public bool TryGetCellIndex(int x, int y, out int index)
	{
		index = GetCellIndex(x, y);
		return index >= 0;
	}

	public bool TryGetCellIndex(Vector2 position, out int index)
	{
		index = GetCellIndex(position);
		return index >= 0;
	}

	public bool TryGetCellCoord(Vector2 position, out Vector2I coord)
	{
		if (position.X < 0f || position.Y < 0f)
		{
			coord = default;
			return false;
		}

		var x = (int)(position.X / CellSize);
		var y = (int)(position.Y / CellSize);
		if (x >= Dimensions.X || y >= Dimensions.Y)
		{
			coord = default;
			return false;
		}

		coord = new Vector2I(x, y);
		return true;
	}

	public Vector2 GetCellPosition(Vector2I coord)
	{
		return new Vector2(coord.X * CellSize, coord.Y * CellSize);
	}

	public bool TryAdd(int x, int y, T item)
	{
		if (!TryGetCellIndex(x, y, out var index))
			return false;

		Cells[index].Add(item);
		return true;
	}

	public bool TryAdd(Vector2 position, T item)
	{
		if (!TryGetCellIndex(position, out var index))
			return false;

		Cells[index].Add(item);
		return true;
	}

	public void ClearGrid()
	{
		for (var i = 0; i < Cells.Length; i++)
			Cells[i].Clear();
	}
}
