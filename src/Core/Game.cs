namespace Game.Core;

public partial class Game : Node
{
    public override void _EnterTree()
    {
#if DEBUG
        GetNode("/root/DebugMenu").Set("style", 2); // Full with graph
#else
        GetNode("/root/DebugMenu").Set("style", 1);
#endif
    }

#if DEBUG
    public override void _Process(double delta)
    {
        if (Input.IsPhysicalKeyPressed(Key.Quoteleft))
        {
            Logger.LogDebug($"{Engine.GetFramesPerSecond()} FPS");
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
