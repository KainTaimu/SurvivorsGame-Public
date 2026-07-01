namespace Game.Players.Controllers;

public partial class PlayerCameraController : Camera2D
{
	[Export]
	public float MinZoom = 0.1f;

	[Export]
	public float MaxZoom = 0.9f;

	public override void _Input(InputEvent @event)
	{
		if (@event is not InputEventMouseButton mouse)
			return;
		if (!Input.IsPhysicalKeyPressed(Key.Ctrl))
			return;

		var zoom = Zoom;
		if (mouse.ButtonIndex == MouseButton.WheelUp)
			zoom += Vector2.One * 0.05f;
		else if (mouse.ButtonIndex == MouseButton.WheelDown)
			zoom -= Vector2.One * 0.05f;

		Zoom = zoom.Clamp(Vector2.One * MinZoom, Vector2.One * MaxZoom);
	}
}