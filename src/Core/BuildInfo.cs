using System.Linq;
using System.Reflection;

namespace Game.Core;

public partial class BuildInfo : CanvasLayer
{
	[Export]
	private Label _label = null!;

	private static Assembly Executable => Assembly.GetExecutingAssembly();

	public override void _Ready()
	{
		var versionAttribute = Executable.GetCustomAttributes<AssemblyInformationalVersionAttribute>().FirstOrDefault();
		if (versionAttribute is null)
		{
			CustomLogger.LogError("Failed to get AssemblyInformationalVersionAttribute");
			QueueFree();
			return;
		}

		_label.Text = versionAttribute.InformationalVersion;
	}
}
