using System.Linq;
using System.Reflection;

public partial class BuildInfo : CanvasLayer
{
    [Export] private Label _label;
    private static Assembly Executable => Assembly.GetExecutingAssembly();

    public override void _Ready()
    {
        var versionAttribute = Executable.GetCustomAttributes<AssemblyInformationalVersionAttribute>().FirstOrDefault();
        if (versionAttribute is null)
        {
            Logger.LogError("Failed to get AssemblyInformationalVersionAttribute");
            QueueFree();
            return;
        }

        _label.Text = versionAttribute.InformationalVersion;
    }
}