using System.Runtime.CompilerServices;

namespace Game.Models;

/// <summary>
/// A moving-window uniform grid recentered around a world-space point.
/// Keeps the underlying <see cref="UniformGrid{T}"/> topology fixed
/// (local space) while mapping world positions via an offset.
/// </summary>
public class UniformGridWorld<T> : IUniformGridWorld<T>
{
	private readonly UniformGrid<T> _grid;

	public readonly int CellSize;
	public readonly Vector2I Dimensions;
	public Vector2 WindowSize { get; }

	public Vector2 Center { get; private set; }
	public Vector2 TopLeft { get; private set; }
	public Rect2 WorldBounds { get; private set; }

	public int Count => _grid.Count;

	public UniformGridWorld(int cellSize, Vector2 windowSize, int initialCapacity = 64)
	{
		if (cellSize <= 0)
		{
			Logger.LogError("Invalid cell size, cellSize must be > 0. Setting as 16px.");
			cellSize = 16;
		}

		CellSize = cellSize;
		WindowSize = windowSize;
		Dimensions = new Vector2I((int)windowSize.X / cellSize, (int)windowSize.Y / cellSize);
		_grid = new UniformGrid<T>(Dimensions.X, Dimensions.Y, initialCapacity);
		Recenter(Vector2.Zero);
	}

	public void Recenter(Vector2 position)
	{
		Center = position;
		TopLeft = position - WindowSize * 0.5f;
		WorldBounds = new Rect2(TopLeft, WindowSize);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool ContainsWorld(Vector2 worldPosition)
	{
		return WorldBounds.HasPoint(worldPosition);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector2I WorldToCell(Vector2 worldPosition)
	{
		var local = worldPosition - TopLeft;
		return new Vector2I(Mathf.FloorToInt(local.X / CellSize), Mathf.FloorToInt(local.Y / CellSize));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector2 CellCenterWorld(int x, int y)
	{
		return TopLeft + new Vector2(x * CellSize, y * CellSize) + Vector2.One * (CellSize * 0.5f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetWorld(Vector2 position, out T result, out GridCellHandle handle)
	{
		var cell = WorldToCell(position);
		if (!IsValidCell(cell.X, cell.Y))
		{
			result = default!;
			handle = default;
			return false;
		}

		return TryGet(cell.X, cell.Y, out result, out handle);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public GridCellHandle AddWorld(Vector2 position, T obj)
	{
		var cell = WorldToCell(position);
		if (!IsValidCell(cell.X, cell.Y))
			return default;
		return Add(cell.X, cell.Y, obj);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ClearCellWorld(Vector2 position)
	{
		var cell = WorldToCell(position);
		if (IsValidCell(cell.X, cell.Y))
			ClearCell(cell.X, cell.Y);
	}

	public void EnsureCapacity(int newCapacity)
	{
		_grid.EnsureCapacity(newCapacity);
	}

	public bool TryGet(int x, int y, out T result, out GridCellHandle handle)
	{
		return _grid.TryGet(x, y, out result, out handle);
	}

	public bool TryGet(GridCellHandle handle, out T result)
	{
		return _grid.TryGet(handle, out result);
	}

	public GridCellHandle Add(int x, int y, T obj)
	{
		return _grid.Add(x, y, obj);
	}

	public void Remove(GridCellHandle targetHandle)
	{
		_grid.Remove(targetHandle);
	}

	public void ClearAll()
	{
		_grid.ClearAll();
	}

	public void ClearCell(int x, int y)
	{
		_grid.ClearCell(x, y);
	}

	public int GetCellCount(int x, int y)
	{
		return _grid.GetCellCount(x, y);
	}

	public bool IsValidCell(int x, int y)
	{
		return _grid.IsValidCell(x, y);
	}

	public CellEnumerator<T> GetEnumerator(int x, int y)
	{
		return _grid.GetEnumerator(x, y);
	}
}
