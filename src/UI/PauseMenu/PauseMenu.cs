using System.Text;
using Game.Players;

namespace Game.UI.Menus;

public partial class PauseMenu : CanvasLayer
{
	[Export]
	private Player _player = null!;

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
		var playerStats = _player.Character.CharacterStats;

		var pType = playerStats.GetType();
		var fields = pType.GetProperties(
			System.Reflection.BindingFlags.Public
				| System.Reflection.BindingFlags.Instance
				| System.Reflection.BindingFlags.DeclaredOnly
		);
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
