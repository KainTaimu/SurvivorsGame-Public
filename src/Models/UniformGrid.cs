using System.Collections.Generic;

namespace Game.Models;

public class UniformGrid<T>
{
    public readonly UniformGridCell<T>[,] Cells;

    public readonly Vector2I Dimensions;

    public readonly int CellSize;

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
        var grid = new UniformGridCell<T>[width, height];

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                grid[x, y] = new UniformGridCell<T>()
                {
                    Index = new Vector2I(x, y),
                    Position = new Vector2I(
                        (cellSize * x) - (startCellX * x),
                        (cellSize * y) - (startCellY * y)
                    ),
                };
            }
        }

        Cells = grid;
        Dimensions = new Vector2I(width, height);
        CellSize = cellSize;
    }

    public UniformGridCell<T>? GetCell(Vector2 position)
    {
        var x = (int)(position.X / CellSize);
        var y = (int)(position.Y / CellSize);

        if (x < 0 || y < 0 || x >= Dimensions.X || y >= Dimensions.Y)
        {
            // Logger.LogWarning($"Invalid cell access at ({x}, {y}) for {position}");
            return null;
        }

        return Cells[x, y];
    }

    public UniformGridCell<T>? GetCell(int x, int y)
    {
        if (x < 0 || y < 0 || x >= Dimensions.X || y >= Dimensions.Y)
        {
            // Logger.LogWarning($"Invalid cell access at ({x}, {y})");
            return null;
        }

        return Cells[x, y];
    }

    public IEnumerable<T> GetCellContent(Vector2 position)
    {
        var x = (int)(position.X / CellSize);
        var y = (int)(position.Y / CellSize);

        if (x < 0 || y < 0 || x >= Dimensions.X || y >= Dimensions.Y)
        {
            // Logger.LogWarning($"Invalid cell access at ({x}, {y}) for {position}");
            yield break;
        }

        var cell = Cells[x, y];
        for (var i = 0; i < cell.Count; i++)
            yield return cell.Array[i];
    }

    public IEnumerable<T> GetCellContent(int x, int y)
    {
        if (x < 0 || y < 0 || x >= Dimensions.X || y >= Dimensions.Y)
        {
            // Logger.LogWarning($"Invalid cell access at ({x}, {y})");
            yield break;
        }

        var cell = Cells[x, y];
        for (var i = 0; i < cell.Count; i++)
            yield return cell.Array[i];
    }

    public void ClearGrid()
    {
        for (var x = 0; x < Dimensions.X; x++)
        for (var y = 0; y < Dimensions.Y; y++)
        {
            Cells[x, y].Clear();
        }
    }
}
