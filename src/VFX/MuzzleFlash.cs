using Game.Items.Offensive;
using Game.Levels.Controllers;
using Game.Players;

namespace Game.UI;

public partial class MuzzleFlash : Node
{
	[Export]
	public BaseOffensive Offensive = null!;

	[Export]
	public PointLight2D Light = null!;

	private Player Player => GameWorld.Instance.MainPlayer;
	private Crosshair Crosshair => Crosshair.Instance!;

	public override void _Ready()
	{
		Offensive.OnAttack += () =>
		{
			Light.GlobalPosition = Player.GlobalPosition;
			Light.GlobalRotation =
				Light.GlobalPosition.AngleToPoint(Crosshair.GlobalSpacePosition)
				+ (90 * Mathf.Pi / 180);
			Light.Show();
			GetTree().CreateTimer(0.05).Timeout += () => Light.Hide();
		};
	}
}
