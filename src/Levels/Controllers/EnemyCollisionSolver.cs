using System.Collections.Generic;
using Game.Core.ECS;
using Game.Models;

namespace Game.Levels.Controllers;

public partial class EnemyCollisionSolver : Node
{
    private const byte GridSize = 64;

    [ExportCategory("Configuration")]
    // [Export(PropertyHint.Range, "0,50,1")]
    private const int _distBeforeShove = 45;

    [Export(PropertyHint.Range, "0,5,0.1")]
    private float _pushAmount = 1.1f;

    [Export]
    public bool DebugEnabled;

    [ExportCategory("Toggles")]
    [Export]
    public bool Enabled = true;

    [Export]
    public byte SubSteps = 6;

    [ExportCategory("Components")]
    [Export]
    private EntityComponentStore _entities = null!;

    private CenteredMovingUniformGrid<(Vector2, int)> _grid = null!;

    private readonly Dictionary<int, Vector2> _writeBuffer = [];

    public override void _Ready()
    {
        var viewport = GetViewport();
        if (viewport is null)
        {
            Logger.LogError("EnemyCollisionSolver: missing viewport.");
            return;
        }

        var windowSize = viewport.GetVisibleRect().Size * 3.0f;
        _grid = new CenteredMovingUniformGrid<(Vector2, int)>(GridSize, windowSize);
        Logger.LogDebug(_grid.Dimensions, _grid.CellSize);

        if (!Enabled)
            return;

        if (DebugEnabled)
            CreateDebugDisplayGridBounds();
    }

    public override void _Process(double delta)
    {
        Process();
    }

    public void Process()
    {
        if (!Enabled)
            return;

        var player = GameWorld.Instance.MainPlayer;
        if (player is null)
            return;

        _writeBuffer.Clear();
        AddObjectsToGrid();
        for (var i = 0; i < SubSteps; i++)
        {
            _grid.Recenter(player.GlobalPosition);
            _grid.ClearGrid();
            AddObjectsToGridFromBuffer();
            SolveCollisions();
        }
        ApplyCollisions();

        if (DebugEnabled)
            UpdateDebug();
    }

    private void ApplyCollisions()
    {
        foreach (var (id, pos) in _writeBuffer)
        {
            _entities.UpdateComponent(id, new PositionComponent(pos) { Position = pos });
        }
    }

    private void AddObjectsToGrid()
    {
        foreach (var (id, pos) in _entities.Query<PositionComponent>())
        {
            if (!_grid.ContainsWorld(pos.Position))
                continue;

            var cell = _grid.GetCellWorld(pos.Position);
            cell?.Add((pos.Position, id));
            _writeBuffer[id] = pos.Position;
        }
    }

    private void AddObjectsToGridFromBuffer()
    {
        foreach (var (id, pos) in _writeBuffer)
        {
            if (!_grid.ContainsWorld(pos))
                continue;

            var cell = _grid.GetCellWorld(pos);
            cell?.Add((pos, id));
        }
    }

    private void SolveCollisions()
    {
        for (var x = 0; x < _grid.Dimensions.X; x++)
        for (var y = 0; y < _grid.Dimensions.Y; y++)
        {
            var cell = _grid.GetCell(x, y);
            if (cell is null || cell.Count <= 1)
                continue;

            SolveCellInternalCollisions(cell);

            SolveCellPairCollisions(cell, _grid.GetCell(x + 1, y)); // E
            SolveCellPairCollisions(cell, _grid.GetCell(x, y + 1)); // S
            SolveCellPairCollisions(cell, _grid.GetCell(x + 1, y + 1)); // SE
            SolveCellPairCollisions(cell, _grid.GetCell(x + 1, y - 1)); // NE
        }
    }

    private void SolveCellInternalCollisions(UniformGridCell<(Vector2 pos, int id)> cell)
    {
        for (var i = 0; i < cell.Count; i++)
        for (var j = i + 1; j < cell.Count; j++)
        {
            SolveCollisionInPlace(cell, i, cell, j);
        }
    }

    private void SolveCellPairCollisions(
        UniformGridCell<(Vector2 pos, int id)> cellA,
        UniformGridCell<(Vector2 pos, int id)>? cellB
    )
    {
        if (cellB is null || cellB.Count == 0)
            return;

        for (var i = 0; i < cellA.Count; i++)
        for (var j = 0; j < cellB.Count; j++)
        {
            SolveCollisionInPlace(cellA, i, cellB, j);
        }
    }

    private void SolveCollisionInPlace(
        UniformGridCell<(Vector2 pos, int id)> cellA,
        int indexA,
        UniformGridCell<(Vector2 pos, int id)> cellB,
        int indexB
    )
    {
        var (posA, idA) = cellA.Array[indexA];
        var (posB, idB) = cellB.Array[indexB];

        if (idA == idB)
            return;

        if (posA.DistanceSquaredTo(posB) >= _distBeforeShove * _distBeforeShove)
            return;

        var direction = posB.DirectionTo(posA);
        if (direction == Vector2.Zero)
            direction = Vector2.Right;

        // NOTE: Delta is accounted for in the Timer that calls Process.
        var push = _distBeforeShove * 0.5f * _pushAmount;
        posA += direction * push;
        posB -= direction * push;

        cellA.Array[indexA] = (posA, idA);
        cellB.Array[indexB] = (posB, idB);

        _writeBuffer[idA] = posA;
        _writeBuffer[idB] = posB;
    }

    private void CreateDebugDisplayGridBounds()
    {
        for (var x = 0; x < _grid.Dimensions.X; x++)
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
                Text = cell.Position + "\n" + cell.Index + "\n" + cell.Count,
                LabelSettings = new LabelSettings { FontColor = new Color(0, 0, 0) },
            };

            AddChild(rect);
            AddChild(text);
        }
    }

    private void UpdateDebug()
    {
        for (var x = 0; x < _grid.Dimensions.X; x++)
        for (var y = 0; y < _grid.Dimensions.Y; y++)
        {
            var cell = _grid.Cells[x, y];
            var text = GetNode<Label>($"text-{cell.Index}");
            text.Text = cell.Position + "\n" + cell.Index + "\n" + cell.Count;
        }
    }
}
