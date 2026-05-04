using Game.Core.Settings;

namespace Game.Core;

public partial class GameSingleton : Node
{
	[Export]
	private GameSettings _gameSettings = null!;

	public static GameSingleton Instance = null!;
	public static GameSettings GameSettings => Instance._gameSettings;

	public override void _EnterTree()
	{
		GetNode("/root/DebugMenu").Set("style", 2); // Full with graph
		Instance = this;
	}

	private void ReadUserSettings() { }

	public override void _Process(double delta)
	{
		if (Input.IsPhysicalKeyPressed(Key.Quoteleft))
		{
			Logger.LogDebug($"{Engine.GetFramesPerSecond()} FPS");
			GetTree().Quit();
			return;
		}
		if (Input.IsPhysicalKeyPressed(Key.F12))
		{
			Logger.LogDebug(
				$"Reloading current scene \"{GetTree().CurrentScene.Name}\""
			);
			GetTree().ReloadCurrentScene();
			return;
		}
	}
}
