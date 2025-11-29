using System.Threading.Tasks;
using SurvivorsGame.Systems;

namespace SurvivorsGame.Levels.Systems;

public partial class PlayerVectorFieldGrid : Node
{
    [Export]
    private int _gridSize = 48;
    private Grid<Vector2> _grid;

    public static PlayerVectorFieldGrid Instance { get; private set; }

    public PlayerVectorFieldGrid()
    {
        if (Instance != null)
        {
            Logger.LogError("Cannot have multiple instances of a singleton!");
            QueueFree();
            return;
        }

        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _Ready()
    {
        _grid = new(_gridSize, 10);
        Logger.LogDebug($"Size: {_grid.Dimensions.X * _grid.Dimensions.Y} ({_grid.Dimensions})");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Engine.GetPhysicsFrames() % 3 == 0)
            return;

        var playerPos = GameWorld.Instance.MainPlayer.GlobalPosition;

        Parallel.For(
            0,
            _grid.Dimensions.X,
            x =>
            {
                for (var y = 0; y < _grid.Dimensions.Y; y++)
                {
                    var cell = _grid.GetCell(x, y);
                    var pos = (Vector2)cell.Position;
                    cell.SingleObject = DirectionToFast(pos, playerPos);
                }
            }
        );
    }

    private static Vector2 DirectionToFast(Vector2 pos, Vector2 target)
    {
        var diff = target - pos;
        var lengthSquared = diff.LengthSquared();
        return diff * MathF.ReciprocalSqrtEstimate(lengthSquared);
    }

    public Vector2 GetDirectionTo(Vector2 v)
    {
        var cell = _grid.GetCell(v);
        if (cell is null)
            return Vector2.Inf; // HACK: Godot's Vector2 is not nullable
        return cell.SingleObject;
    }
}
