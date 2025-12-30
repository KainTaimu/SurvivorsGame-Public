namespace Game.UI.Menus;

public partial class PauseController : Node
{
    [Signal]
    public delegate void OnPauseEventHandler();

    [Signal]
    public delegate void OnUnpauseEventHandler();

    public bool IsPaused;

    public Node? LockedBy { get; private set; }
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

        EmitSignal(SignalName.OnPause);
        Tree.Paused = true;
        IsPaused = Tree.Paused;
    }

    public void Unpause(Node locker)
    {
        if (locker != LockedBy && LockedBy is not null)
        {
            return;
        }

        EmitSignal(SignalName.OnUnpause);
        Tree.Paused = false;
        IsPaused = Tree.Paused;
    }
}
