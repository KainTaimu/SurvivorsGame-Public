namespace Game.Players.Controllers;

public partial class PlayerCameraController : Camera2D
{
	[Export]
	public float MinZoom = 0.1f;

	[Export]
	public float MaxZoom = 0.9f;

	public override void _Input(InputEvent @event)
	{
		var zoom = Zoom;

		if (@event.IsActionPressed("ZOOM_IN"))
			zoom += Vector2.One * 0.05f;
		else if (@event.IsActionPressed("ZOOM_OUT"))
			zoom -= Vector2.One * 0.05f;

		Zoom = zoom.Clamp(Vector2.One * MinZoom, Vector2.One * MaxZoom);
	}
}
