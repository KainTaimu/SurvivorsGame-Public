using System.Reflection;
using System.Text;
using Game.Utils;

namespace Game.Players.Controllers;

public partial class DebugClassInspector : CanvasLayer
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
	} = true;

	[Export]
	public BindingFlags BindingFlags = BindingFlags.Instance;

	[Export]
	private PanelContainer _panel = null!;

	[Export]
	private Label _label = null!;

	public override void _Process(double delta)
	{
		if (!Enabled)
			return;

		var target = GetParentOrNull<Node>();
		if (target is null)
		{
			Hide();
			return;
		}
		if (!Visible)
			Show();

		var s = new StringBuilder();
		s.AppendLine(ClassInspector.GetClassFieldsString(target, BindingFlags));
		s.AppendLine(ClassInspector.GetClassPropertiesString(target, BindingFlags));
		_label.Text = s.ToString();
	}
}
