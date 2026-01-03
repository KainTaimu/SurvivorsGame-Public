namespace Game.Core;

public partial class Game : Node
{
#if DEBUG
    public override void _EnterTree()
    {
        GetNode("/root/DebugMenu").Set("style", 2);
    }

    public override void _Process(double delta)
    {
        if (Input.IsPhysicalKeyPressed(Key.Quoteleft))
        {
            GetTree().Quit();
            return;
        }
        if (Input.IsPhysicalKeyPressed(Key.F12))
        {
            Logger.LogDebug($"Reloading current scene \"{GetTree().CurrentScene.Name}\"");
            GetTree().ReloadCurrentScene();
            return;
        }
    }
#endif
}
