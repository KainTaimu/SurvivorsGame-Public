namespace Game.Players.Controllers;

public partial class PlayerCameraController : SignalCamera2D
{
	[Export]
	public float MinZoom = 0.15f;

	[Export]
	public float MaxZoom = 0.9f;

	public override void _Input(InputEvent @event)
	{
		if (!@event.IsActionPressed("ZOOM_IN") && !@event.IsActionPressed("ZOOM_OUT"))
			return;

		var zoom = CurrentZoom;

		if (@event.IsActionPressed("ZOOM_IN"))
			zoom += 0.05f;
		else if (@event.IsActionPressed("ZOOM_OUT"))
			zoom -= 0.05f;

		CurrentZoom = float.Clamp(zoom, MinZoom, MaxZoom);
	}
}
