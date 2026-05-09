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

	public static PauseController? Instance { get; private set; }

	public override void _EnterTree()
	{
		Instance = this;
	}

	public void Lock(Node locker)
	{
		if (locker != LockedBy && LockedBy is not null)
			return;
		if (locker.ProcessMode != ProcessModeEnum.Always)
		{
			Logger.LogError("Locker must have ProcessMode set to Always to lock the pause controller.");
			return;
		}

		LockedBy = locker;
	}

	public void Unlock(Node locker)
	{
		if (locker != LockedBy && LockedBy is not null)
			return;

		LockedBy = null;
	}

	public void Pause(Node locker)
	{
		if (locker != LockedBy && LockedBy is not null)
			return;

		EmitSignal(SignalName.OnPause);
		Tree.Paused = true;
		IsPaused = Tree.Paused;
	}

	public void Unpause(Node locker)
	{
		if (locker != LockedBy && LockedBy is not null)
			return;

		EmitSignal(SignalName.OnUnpause);
		Tree.Paused = false;
		IsPaused = Tree.Paused;
	}
}
