using System.Text;
using Game.Utils;

namespace Game.Players.Controllers;

public partial class DebugPlayerStatusEffect : CanvasLayer
{
	[Export]
	public PlayerStatusEffectController StatusEffectController = null!;

	[Export]
	public Label Label = null!;

	public override void _Process(double delta)
	{
		var target = StatusEffectController;
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
