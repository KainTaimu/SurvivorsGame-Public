using System.Text;
using Game.Players.Controllers;

namespace Game.Utils;

public partial class ItemDebugInfo : CanvasLayer
{
	[Export]
	public Node? Target;

	[Export]
	public AbstractPlayerWeaponController? PlayerWeaponController;

	[Export]
	public Label Label = null!;

	public override void _UnhandledKeyInput(InputEvent @event)
	{
		if (@event is InputEventKey { Pressed: true, Keycode: Key.F4 })
		{
			if (Visible)
				Hide();
			else
				Show();
		}
	}

	public override void _Process(double delta)
	{
		var target = Target ?? PlayerWeaponController?.PrimaryAttack as Node;
		if (target is null)
		{
			Hide();
			return;
		}
		if (!Visible)
			Show();

		var s = new StringBuilder();
		s.AppendLine(
			ClassInspector.GetClassFieldsString(
				target,
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
			)
		);
		s.AppendLine(
			ClassInspector.GetClassPropertiesString(
				target,
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
			)
		);
		Label.Text = s.ToString();
	}
}
