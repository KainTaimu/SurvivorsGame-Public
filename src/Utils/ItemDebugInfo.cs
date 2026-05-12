using System.Text;
using Game.Players.Controllers;
using Node = Godot.Node;

namespace Game.Utils;

public partial class ItemDebugInfo : CanvasLayer
{
	[Export]
	public bool Enabled
	{
		get;
		set
		{
			field = value;
			Visible = field;
		}
	}

	[Export]
	public Node? Target;

	[Export]
	public AbstractPlayerWeaponController? PlayerWeaponController;

	[Export]
	public Label Label = null!;

	public override void _Ready()
	{
		Visible = Enabled;
	}

	public override void _Process(double delta)
	{
		if (!Enabled)
			return;

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
