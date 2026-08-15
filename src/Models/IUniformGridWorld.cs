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