namespace SurvivorsGame.Systems;

public partial class Game : Node
{
    public Game()
    {
        if (Instance != null)
        {
            Logger.LogError("Cannot have multiple instances of a singleton!");
            QueueFree();
            return;
        }

        Instance = this;
    }

    public static Game Instance { get; private set; }
    public static bool IsDebugEnabled { get; private set; } = true;

    public override void _Process(double delta)
    {
        if (Input.IsPhysicalKeyPressed(Key.Quoteleft))
        {
            GetTree().Quit();
        }
    }
}

