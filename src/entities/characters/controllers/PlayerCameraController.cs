using SurvivorsGame.Systems;

namespace SurvivorsGame.Entities.Characters;

public partial class PlayerCameraController : Node
{
    [Export]
    public Camera2D Camera { get; private set; }

    public override void _Ready()
    {
        Camera.LimitLeft = 0;
        Camera.LimitTop = 0;
        Camera.LimitRight = (int)GameWorld.Instance.CurrentLevel.PixelSize.X;
        Camera.LimitBottom = (int)GameWorld.Instance.CurrentLevel.PixelSize.Y;
    }
}

