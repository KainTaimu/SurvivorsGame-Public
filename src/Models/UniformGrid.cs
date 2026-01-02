using System.Collections.Generic;

namespace Game.Models;

public class UniformCollisionGrid<T> : UniformGrid<T>
{
    public UniformCollisionGrid(
        int cellSize,
        int sizeX,
        int sizeY,
        int startX = 0,
        int startY = 0,
        int padding = 0
    )
        : base(cellSize, startX, startY, sizeX, sizeY) { }
}

public class UniformGrid<T>
{
    private readonly UniformGridCell[,] _cells;

    protected readonly Vector2I _dimensions;

    protected readonly int _cellSize;

    public UniformGrid(int cellSize, int sizeX, int sizeY, int startX = 0, int startY = 0)
    {
        if (cellSize <= 0)
        {
            Logger.LogError("Invalid cell size, cellSize must be > 0. Setting as 16px.");
            cellSize = 16;
        }

        var startCellX = startX / cellSize;
        var startCellY = startY / cellSize;
        var width = sizeX / cellSize;
        var height = sizeY / cellSize;
        var grid = new UniformGridCell[width, height];

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                grid[x, y] = new UniformGridCell()
                {
                    Index = new Vector2I(x, y),
                    Position = new Vector2I(
                        (cellSize * x) - (startCellX * x),
                        (cellSize * y) - (startCellY * y)
                    ),
                };
            }
        }

        _cells = grid;
        _dimensions = new Vector2I(width, height);
        _cellSize = cellSize;
    }

    public IEnumerable<T> GetCellContent(Vector2 position)
    {
        var x = (int)(position.X / _cellSize);
        var y = (int)(position.Y / _cellSize);

        if (x < 0 || y < 0 || x >= _dimensions.X || y >= _dimensions.Y)
        {
            Logger.LogWarning($"Invalid cell access at ({x}, {y}) for {position}");
            yield break;
        }

        var cell = _cells[x, y];
        for (var i = 0; i < cell.Count; i++)
            yield return cell.Array[i];
    }

    public IEnumerable<T> GetCellContent(int x, int y)
    {
        if (x < 0 || y < 0 || x >= _dimensions.X || y >= _dimensions.Y)
        {
            Logger.LogWarning($"Invalid cell access at ({x}, {y})");
            yield break;
        }

        var cell = _cells[x, y];
        for (var i = 0; i < cell.Count; i++)
            yield return cell.Array[i];
    }

    private class UniformGridCell
    {
        private const int MAX_SIZE = 32;

        public required Vector2I Index;
        public required Vector2I Position;

        public readonly T[] Array = new T[MAX_SIZE];

        public int Count
        {
            get => _count;
            set
            {
                var clamped = Math.Clamp(value, 0, MAX_SIZE);
                _count = clamped;
            }
        }

        private int _count;

        // Drops obj silenty by design
        public void Add(T obj)
        {
            if (Count >= MAX_SIZE)
                return;

            Array[Count] = obj;
            Count++;
        }

        public void Clear()
        {
            Count = 0;
        }
    }
}
