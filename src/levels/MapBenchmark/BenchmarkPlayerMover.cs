using SurvivorsGame.Systems;

namespace SurvivorsGame.Levels.Benchmark;

public partial class BenchmarkPlayerMover : Node
{
    private Side _playerSide = Side.Left;

    private double _t;

    [Export]
    private double _teleportTime = 5;

    private static float LevelVerticalMiddle => GameWorld.Instance.CurrentLevel.PixelSize.Y / 2;
    private static float LevelHorizontalMiddle => GameWorld.Instance.CurrentLevel.PixelSize.X / 2;

    public override void _Ready() { }

    public override void _Process(double delta)
    {
        if (_t > 0)
        {
            _t -= delta;
            return;
        }

        _t = _teleportTime;

        _playerSide = _playerSide switch
        {
            Side.Top => Side.Right,
            Side.Right => Side.Bottom,
            Side.Bottom => Side.Left,
            Side.Left => Side.Top,
            _ => _playerSide,
        };

        TeleportPlayerToSide(_playerSide);
    }

    private void TeleportPlayerToSide(Side side)
    {
        Vector2 tpPos = default;
        switch (side)
        {
            case Side.Left:
                tpPos = new Vector2(0, LevelVerticalMiddle);
                break;
            case Side.Right:
                tpPos = new Vector2(
                    GameWorld.Instance.CurrentLevel.PixelSize.X,
                    LevelVerticalMiddle
                );
                break;
            case Side.Top:
                tpPos = new Vector2(LevelHorizontalMiddle, 0);
                break;
            case Side.Bottom:
                tpPos = new Vector2(
                    LevelHorizontalMiddle,
                    GameWorld.Instance.CurrentLevel.PixelSize.Y
                );
                break;
        }

        GameWorld.Instance.MainPlayer.SetPosition(tpPos);
    }

    private enum Side
    {
        Left,

        Right,

        Top,

        Bottom,
    }
}

