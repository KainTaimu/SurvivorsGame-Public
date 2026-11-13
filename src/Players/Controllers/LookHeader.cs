using Game.UI;

namespace Game.Players.Controllers;

public partial class LookHeader : Node2D
{
	private Crosshair? Crosshair => Crosshair.Instance;

	public override void _Process(double delta)
	{
		if (Crosshair is null)
			return;
		GlobalRotation = GlobalPosition.AngleToPoint(Crosshair.GlobalSpacePosition) - 90 * Mathf.Pi / 180;
	}
}
