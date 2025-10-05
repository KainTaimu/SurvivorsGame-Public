using SurvivorsGame.UI.Menus;
using PauseController = SurvivorsGame.UI.Menus.PauseController;

namespace SurvivorsGame.UI;

public partial class Crosshair : Node2D
{
    public static Crosshair Instance { get; private set; }

    [Export(PropertyHint.Range, "0,5,0.25")]
    public float CrosshairSize = 4;

    [Export] private AnimatedSprite2D _crosshairSprite;

    public Crosshair()
    {
        if (Instance != null)
        {
            Logger.LogError("Cannot have multiple instances of a singleton!");
            QueueFree();
            return;
        }

        Instance = this;
    }

    public override void _Ready()
    {
        PauseController.Instance.Paused += HideCrosshair;
        PauseController.Instance.Unpaused += ShowCrosshair;

        if (!Engine.IsEditorHint())
        {
            Input.SetMouseMode(Input.MouseModeEnum.Hidden);
        }
    }

    public override void _Input(InputEvent @event)
    {
        switch (@event)
        {
            case InputEventMouseMotion mouseMotion:
                Position = mouseMotion.Position;
                break;

            case InputEventMouseButton mouse:
            {
                if (mouse.ButtonIndex == MouseButton.Left)
                {
                    _crosshairSprite.Play();
                }

                break;
            }
        }
    }

    public void ChangeCrosshairSize(float newSize)
    {
        _crosshairSprite.Scale = new Vector2(1, 1) * newSize;
    }

    public void ShowCrosshair()
    {
        Show();
        Position = GetViewport().GetMousePosition();
        Input.SetMouseMode(Input.MouseModeEnum.Hidden);
    }

    public void HideCrosshair()
    {
        Hide();
        Input.SetMouseMode(Input.MouseModeEnum.Visible);
    }
}