using System.Reflection;
using System.Text;
using Game.Utils;

namespace Game.Players.Controllers;

public partial class DebugClassInspector : CanvasLayer
{
	[Export]
	public BindingFlags BindingFlags = BindingFlags.Instance;

	[Export]
	private Label _label = null!;

	public override void _Process(double delta)
	{
		var target = GetParentOrNull<Node>();
		if (target is null)
			return;
		var s = new StringBuilder();
		s.AppendLine(ClassInspector.GetClassFieldsString(target, BindingFlags));
		s.AppendLine(ClassInspector.GetClassPropertiesString(target, BindingFlags));
		_label.Text = s.ToString();
	}
}
