using Game.UI;

namespace Game.Players;

public partial class LookLight : PointLight2D
{
	private Crosshair? Crosshair => Crosshair.Instance;
	private Viewport? Viewport => GetViewport();

	public override void _Process(double delta)
	{
		if (Crosshair is null)
			return;
		GlobalRotation =
			GlobalPosition.AngleToPoint(Crosshair.GlobalSpacePosition)
			+ (90 * Mathf.Pi / 180);
	}
}
