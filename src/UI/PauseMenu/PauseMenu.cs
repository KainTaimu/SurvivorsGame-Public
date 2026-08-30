using Game.Core;
using Game.Levels.Controllers;
using Game.Players;

namespace Game.UI.Menus;

public partial class PauseMenu : CanvasLayer
{
	private Player Player => GameWorld.Instance.MainPlayer;

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed(InputMapNames.UiCancel))
			ToggleShow();
	}

	private void ToggleShow()
	{
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
