namespace Game.Core;

public partial class PauseController : Node
{
	[Signal]
	public delegate void OnPauseEventHandler();

	[Signal]
	public delegate void OnUnpauseEventHandler();

	[Export]
	public bool ThrottleFpsWhenPaused = true;

	[Export]
	public int ThrottledFpsTarget = 60;

	public bool IsPaused;

	public Node? LockedBy { get; private set; }
	private SceneTree Tree => GetTree();

	public static PauseController Instance { get; private set; } = null!;

	public override void _EnterTree()
	{
		Instance = this;
	}

	public void Lock(Node locker)
	{
		if (locker != LockedBy && LockedBy is not null)
		{
			// Logger.LogError($"Cannot lock the pause controller while it is already locked by another node {LockedBy}.");
			return;
		}
		if (locker.ProcessMode != ProcessModeEnum.Always)
		{
			CustomLogger.LogError("Locker must have ProcessMode set to Always to lock the pause controller.");
			return;
		}

		LockedBy = locker;
	}

	public void Unlock(Node locker)
	{
		if (locker != LockedBy && LockedBy is not null)
		{
			// Logger.LogError($"Cannot unlock the pause controller while it is not locked by {locker}.");
			return;
		}

		LockedBy = null;
	}

	public void Pause(Node locker)
	{
		if (locker != LockedBy && LockedBy is not null)
		{
			// Logger.LogError($"Cannot pause the pause controller while it is locked by another node {LockedBy}.");
			return;
		}

		EmitSignal(SignalName.OnPause);
		Tree.Paused = true;
		IsPaused = Tree.Paused;
		if (ThrottleFpsWhenPaused)
			Engine.MaxFps = ThrottledFpsTarget;
	}

	public void Unpause(Node locker)
	{
		if (locker != LockedBy && LockedBy is not null)
		{
			// Logger.LogError($"Cannot unpause the pause controller while it is locked by another node {LockedBy}.");
			return;
		}

		EmitSignal(SignalName.OnUnpause);
		Tree.Paused = false;
		IsPaused = Tree.Paused;
		if (ThrottleFpsWhenPaused)
			Engine.MaxFps = 0;
	}
}
