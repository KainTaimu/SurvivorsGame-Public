using System.Reflection;
using System.Text;
using Game.Levels.Controllers;
using Game.Players;

namespace Game.UI.Menus;

public partial class PauseMenu : CanvasLayer
{
	private Player Player => GameWorld.Instance.MainPlayer;

	[Export]
	private PauseController _pauseController = null!;

	[Export]
	private Label _playerStats = null!;

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
		var isPaused = _pauseController.IsPaused;

		_pauseController.Lock(this);
		switch (!isPaused)
		{
			case true:
				UpdatePlayerStats();
				Show();
				_pauseController.Pause(this);
				break;

			case false:
				Hide();
				_pauseController.Unpause(this);
				break;
		}

		_pauseController.Unlock(this);
	}

	private void UpdatePlayerStats()
	{
		var statString = new StringBuilder();
		var playerStats = Player.Character.CharacterStats;

		var pType = playerStats.GetType();
		var fields = pType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
		foreach (var f in fields)
		{
			var value = f.GetValue(playerStats);
			if (value is null)
				continue;
			statString.AppendLine($"{f.Name}: {value.ToString()}");
		}

		_playerStats.Text = statString.ToString();
	}
}
