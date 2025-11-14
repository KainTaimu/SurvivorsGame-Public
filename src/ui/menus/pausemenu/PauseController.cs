namespace SurvivorsGame.UI.Menus;

public partial class PauseController : Node
{
    [Signal]
    public delegate void PausedEventHandler();

    [Signal]
    public delegate void UnpausedEventHandler();

    public bool IsPaused;

    public PauseController()
    {
        if (Instance != null)
        {
            Logger.LogError("Cannot have multiple instances of a singleton!");
            QueueFree();
            return;
        }

        Instance = this;
    }

    public static PauseController Instance { get; private set; }
    public Node LockedBy { get; private set; }
    private SceneTree Tree => GetTree();

    public void Lock(Node locker)
    {
        if (locker != LockedBy && LockedBy is not null)
        {
            return;
        }

        LockedBy = locker;
    }

    public void Unlock(Node locker)
    {
        if (locker != LockedBy && LockedBy is not null)
        {
            return;
        }

        LockedBy = null;
    }

    public void Pause(Node locker)
    {
        if (locker != LockedBy && LockedBy is not null)
        {
            return;
        }

        EmitSignal(nameof(Paused));
        Tree.Paused = true;
        IsPaused = Tree.Paused;
    }

    public void Unpause(Node locker)
    {
        if (locker != LockedBy && LockedBy is not null)
        {
            return;
        }

        EmitSignal(nameof(Unpaused));
        Tree.Paused = false;
        IsPaused = Tree.Paused;
    }
}

