namespace Game.Models;

/// <summary>
/// A moving-window uniform grid that is recentered around a world-space point.
///
/// This keeps the underlying <see cref="UniformGrid{T}"/> topology fixed (local space),
/// while mapping world positions into the grid via an offset.
/// </summary>
public class CenteredMovingUniformGrid<T> : UniformGrid<T>
	where T : struct
{
	public Vector2 WindowSize { get; }

	public Vector2 Center { get; private set; }

	/// <summary>
	/// World-space top-left corner of the grid window.
	/// </summary>
	public Vector2 TopLeft { get; private set; }

	public Rect2 WorldBounds { get; private set; }

	public CenteredMovingUniformGrid(int cellSize, Vector2 windowSize)
		: base(cellSize, (int)windowSize.X, (int)windowSize.Y)
	{
		WindowSize = windowSize;
		Recenter(Vector2.Zero);
	}

	public void Recenter(Vector2 center)
	{
		Center = center;
		TopLeft = center - WindowSize * 0.5f;
		WorldBounds = new Rect2(TopLeft, WindowSize);
	}

	public bool ContainsWorld(Vector2 worldPosition)
	{
		return WorldBounds.HasPoint(worldPosition);
	}

	public int GetCellIndexWorld(Vector2 worldPosition)
	{
		return GetCellIndex(worldPosition - TopLeft);
	}

	public bool TryGetCellIndexWorld(Vector2 worldPosition, out int index)
	{
		return TryGetCellIndex(worldPosition - TopLeft, out index);
	}

	public bool TryGetCellCoordWorld(Vector2 worldPosition, out Vector2I coord)
	{
		return TryGetCellCoord(worldPosition - TopLeft, out coord);
	}

	public bool TryAddWorld(Vector2 worldPosition, T item)
	{
		return TryAdd(worldPosition - TopLeft, item);
	}
}
