namespace SurvivorsGame.ui;

public partial class FpsCounter : Node
{
    public override void _Ready()
    {
        GetNode("/root/DebugMenu").Set("style", 2);
    }
}
