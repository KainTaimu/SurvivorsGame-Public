// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace Game.Core;

public partial class PauseController : Node
{
	[Signal]
	public delegate void OnPauseEventHandler();

	[Signal]
	public delegate void OnUnpauseEventHandler();

	[Signal]
	public delegate void OnPauseToggledEventHandler(bool toggle);

	[Export]
	public bool ThrottleFpsWhenPaused = true;

	[Export]
	public int ThrottledFpsTarget = 60;

	public bool IsLocked => LockedBy is not null;
	public bool IsPaused { get; private set; }

	public Node? LockedBy { get; private set; }
	private SceneTree Tree => GetTree();

	public static PauseController Instance { get; private set; } = null!;

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _ExitTree()
	{
		Instance = null!;
	}

	/// <summary>
	/// Returns true if lock successful
	/// </summary>
	public bool Lock(Node locker)
	{
		if (locker != LockedBy && LockedBy is not null)
		{
			// Logger.LogError($"Cannot lock the pause controller while it is already locked by another node {LockedBy}.");
			return false;
		}

		if (locker.ProcessMode != ProcessModeEnum.Always)
		{
			CustomLogger.LogError("Locker must have ProcessMode set to Always to lock the pause controller.");
			return false;
		}

		LockedBy = locker;
		return true;
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
		EmitSignalOnPauseToggled(true);
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
		EmitSignalOnPauseToggled(false);
		Tree.Paused = false;
		IsPaused = Tree.Paused;
		if (ThrottleFpsWhenPaused)
			Engine.MaxFps = 0;
	}
}
