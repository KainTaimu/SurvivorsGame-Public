namespace Game.Players.Controllers;

public partial class PlayerCameraController : Camera2D
{
    [Export]
    public float _minZoom = 0.1f;

    [Export]
    public float _maxZoom = 0.9f;

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouse)
            return;
        var zoom = Zoom;
        if (mouse.ButtonIndex == MouseButton.WheelUp)
            zoom += Vector2.One * 0.1f;
        else if (mouse.ButtonIndex == MouseButton.WheelDown)
            zoom -= Vector2.One * 0.1f;

        Zoom = zoom.Clamp(Vector2.One * _minZoom, Vector2.One * _maxZoom);
    }
}
