using Game.Core;
using Game.Levels.Controllers;
using Game.Players;

namespace Game.UI.Menus;

public partial class PauseMenu : CanvasLayer
{
	private Player Player => GameWorld.Instance.MainPlayer;

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey { Pressed: true } keyEvent)
		{
			switch (keyEvent.Keycode)
			{
				case Key.Escape:
					ToggleShow();
					break;
			}
		}
	}

	private void ToggleShow()
	{
		var isPaused = PauseController.Instance.IsPaused;

		PauseController.Instance.Lock(this);
		switch (!isPaused)
		{
			case true:
				Show();
				PauseController.Instance.Pause(this);
				break;

			case false:
				Hide();
				PauseController.Instance.Unpause(this);
				break;
		}

		PauseController.Instance.Unlock(this);
	}
}
