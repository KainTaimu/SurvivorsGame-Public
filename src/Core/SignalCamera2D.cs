namespace Game.Players.Controllers;

public partial class SignalCamera2D : Camera2D
{
	[Signal]
	public delegate void OnCurrentZoomChangedEventHandler(float newValue, float oldValue);

	[Export]
	public float CurrentZoom
	{
		get;
		set
		{
			var old = field;
			field = value;
			Zoom = Vector2.One * field;
			EmitSignalOnCurrentZoomChanged(value, old);
		}
	} = 0.7f;
}