// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Game.Core;
using Game.Levels.Controllers;
using Game.Players;

namespace Game.UI.Menus;

[GlobalClass]
public partial class PauseMenu : CanvasLayer
{
	private Player Player => GameWorld.Instance.MainPlayer;

	public override void _Input(InputEvent @event)
	{
		if (!@event.IsActionPressed(InputMapNames.UiCancel))
			return;

		ToggleShow();
	}

	private void ToggleShow()
	{
		if (PauseController.Instance.IsLocked && PauseController.Instance.LockedBy != this)
			return;
		var isPaused = PauseController.Instance.IsPaused;

		switch (!isPaused)
		{
			case true:
				Show();
				PauseController.Instance.Lock(this);
				PauseController.Instance.Pause(this);
				break;

			case false:
				Hide();
				PauseController.Instance.Unpause(this);
				PauseController.Instance.Unlock(this);
				break;
		}
	}
}
