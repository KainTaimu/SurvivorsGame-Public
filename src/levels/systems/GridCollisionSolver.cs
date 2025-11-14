using SurvivorsGame.Systems;

namespace SurvivorsGame.Levels.Systems;

public partial class GridCollisionSolver : Node
{
    private const byte GridSize = 64;

    [ExportCategory("Configuration")]
    [Export(PropertyHint.Range, "0,100,1")]
    private float _distBeforeShove = 45;

    private Grid _grid;

    [Export(PropertyHint.Range, "0,5,0.1")]
    private float _pushAmount = 1.1f;

    [Export]
    public bool DebugEnabled;

    [ExportCategory("Toggles")]
    [Export]
    public bool Enabled = true;

    [Export]
    public byte SubSteps = 6;

    public override void _Ready()
    {
        if (!Enabled)
        {
            return;
        }

        _grid = new Grid(GridSize);

        if (DebugEnabled)
        {
            CreateDebugDisplayGridBounds();
        }
    }

    private void Process()
    {
        if (!Enabled || !GameWorld.Instance.MainPlayer.Alive)
        {
            return;
        }

        for (var i = 0; i < SubSteps; i++)
        {
            AddObjectsToGrid();
            SolveCollisions();
        }

#if DEBUG
        if (DebugEnabled)
        {
            UpdateDebug();
        }
#endif
    }

    // Can be made async when spawner spawns an enemy
    private void AddObjectsToGrid()
    {
        _grid.Clear();
        foreach (var enemy in GameWorld.Instance.Enemies)
        {
            var position = enemy.Position;
            var cell = _grid.GetCell(position);

            cell?.AddObject(enemy);
        }
    }

    private void SolveCollisions()
    {
        ProcessCell();
    }

    private void ProcessCell()
    {
        var gridX = _grid.Dimensions.X;
        var gridY = _grid.Dimensions.Y;
        for (var x = 1; x < gridX; x++)
        {
            for (var y = 1; y < gridY; y++)
            {
                var firstCell = _grid.GetCell(x, y);
                if (firstCell.Objects.Count < 2)
                {
                    continue;
                }

                CheckCellCollisions(firstCell); // Current cell
                CheckCellCollisions(_grid.GetCell(x + 1, y)); // E
                CheckCellCollisions(_grid.GetCell(x + 1, y + 1)); // SE
                CheckCellCollisions(_grid.GetCell(x, y + 1)); // S
                CheckCellCollisions(_grid.GetCell(x - 1, y + 1)); // SW
                CheckCellCollisions(_grid.GetCell(x - 1, y)); // W
                CheckCellCollisions(_grid.GetCell(x - 1, y - 1)); // NW
                CheckCellCollisions(_grid.GetCell(x, y - 1)); // N
                CheckCellCollisions(_grid.GetCell(x + 1, y - 1)); // NE
            }
        }
    }

    private void CheckCellCollisions(GridCell cell)
    {
        if (cell is null)
        {
            return;
        }

        foreach (var cellObject1 in cell.Objects)
        {
            foreach (var cellObject2 in cell.Objects)
            {
                SolveCollision(cellObject1, cellObject2);
            }
        }
    }

    private void SolveCollision(Node2D cellObject1, Node2D cellObject2)
    {
        if (cellObject1 == cellObject2)
        {
            return;
        }

        if (!IsInstanceValid(cellObject1) || !IsInstanceValid(cellObject2))
        {
            return;
        }

        var distance = cellObject2.Position.DistanceSquaredTo(cellObject1.Position);
        if (distance < _distBeforeShove * _distBeforeShove)
        {
            var direction = cellObject2.Position.DirectionTo(cellObject1.Position);

            cellObject1.Position += direction / 2 * _pushAmount;
            cellObject2.Position -= direction / 2 * _pushAmount;
        }
    }

    private void CreateDebugDisplayGridBounds()
    {
        for (var x = 0; x < _grid.Dimensions.X; x++)
        {
            for (var y = 0; y < _grid.Dimensions.Y; y++)
            {
                var cell = _grid.Cells[x, y];
                var rect = new ReferenceRect
                {
                    Name = $"rect-{cell.Index}",
                    Size = new Vector2(GridSize, GridSize),
                    Position = cell.Position,
                    Visible = true,
                    EditorOnly = false,
                };

                var text = new Label
                {
                    Name = $"text-{cell.Index}",
                    Scale = new Vector2(0.75f, 0.75f),
                    Position = cell.Position,
                    Text = cell.Position + "\n" + cell.Index + "\n" + cell.Objects.Count,
                    LabelSettings = new LabelSettings { FontColor = new Color(0, 0, 0) },
                };

                AddChild(rect);
                AddChild(text);
            }
        }
    }

    private void UpdateDebug()
    {
        for (var x = 0; x < _grid.Dimensions.X; x++)
        {
            for (var y = 0; y < _grid.Dimensions.Y; y++)
            {
                var cell = _grid.GetCell(x, y);
                var text = GetNode<Label>($"text-{cell.Index}");
                text.Text = cell.Position + "\n" + cell.Index + "\n" + cell.Objects.Count;
            }
        }
    }
}

