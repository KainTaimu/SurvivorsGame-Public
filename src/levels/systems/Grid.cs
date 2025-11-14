using SurvivorsGame.Systems;

namespace SurvivorsGame.Levels.Systems;

public class Grid
{
    private const int Padding = 5;

    private readonly int _cellSize;

    public readonly GridCell[,] Cells;

    public readonly Vector2I Dimensions;

    public Grid(int cellSize) // Generate cells
    {
        var gameWorldSize = GameWorld.Instance.CurrentLevel.PixelSize;

        _cellSize = cellSize;

        var width = (int)Math.Ceiling(gameWorldSize.X / _cellSize) + Padding;
        var height = (int)Math.Ceiling(gameWorldSize.Y / _cellSize) + Padding;
        Cells = new GridCell[width, height];

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                var cell = new GridCell
                {
                    Position = new Vector2I(
                        _cellSize * x - _cellSize * 2,
                        _cellSize * y - _cellSize * 2
                    ),
                    Index = new Vector2I(x, y),
                };
                Cells[x, y] = cell;
            }
        }

        Dimensions = new Vector2I(Cells.GetUpperBound(0), Cells.GetUpperBound(1));
    }

    public void Clear()
    {
        for (var i = 0; i < Dimensions.X; i++)
        {
            for (var j = 0; j < Dimensions.Y; j++)
            {
                Cells[i, j].Clear();
            }
        }
    }

    public GridCell GetCell(Vector2 position)
    {
        var ix = (int)Math.Round(position.X / _cellSize) + 2;
        var iy = (int)Math.Round(position.Y / _cellSize) + 2;

        if (ix < 0 || iy < 0 || ix > Dimensions.X || iy > Dimensions.Y)
        {
            return null;
        }

        return Cells[ix, iy];
    }

    public GridCell GetCell(int x, int y)
    {
        if (x < 0 || y < 0 || x > Dimensions.X || y > Dimensions.Y)
        {
            return null;
        }

        return Cells[x, y];
    }
}

