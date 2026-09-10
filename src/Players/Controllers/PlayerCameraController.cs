namespace Game.Players.Controllers;

public partial class PlayerCameraController : SignalCamera2D
{
	[Export]
	public float MinZoom = 0.15f;

	[Export]
	public float MaxZoom = 0.9f;

	private float _shake;

	public override void _Process(double delta)
	{
		_shake = (float)Mathf.Max(0, _shake - delta);
		const float a = 15;
		const float b = 90;
		const float m = 0.1f;
		Rotation = Mathf.Pow(Mathf.E, (-a * (3 - _shake))) * Mathf.Cos(b * (3 - _shake)) * m;
	}

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

	public void Shake()
	{
		_shake = 3;
	}
}