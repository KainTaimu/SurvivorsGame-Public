using System.Text;
using Game.Core.Settings;
using Newtonsoft.Json;

namespace Game.Core;

public partial class GameSingleton : Node
{
	[Export]
	private GameSettings GameSettings
	{
		get;
		set
		{
			field = value;
			GameSettings.Instance = field;
		}
	} = null!;

	public static GameSingleton Instance = null!;

	public const string SETTINGS_FILE = "user://settings.json";
	public const string DEFAULT_SETTINGS = "uid://21pymi1puoaf";

	public override void _EnterTree()
	{
		GetNode("/root/DebugMenu").Set("style", 2); // Full with graph
		Instance = this;
		ReadUserSettings();
	}

	public override void _ExitTree()
	{
		WriteUserSettings(GameSettings);
	}

	private void WriteUserSettings(GameSettings settings)
	{
		var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
		using var settingsFile = FileAccess.Open(
			"user://settings.json",
			FileAccess.ModeFlags.Write
		);
		settingsFile.StoreLine(json);
	}

	private void WriteDefaultUserSettings()
	{
		var defaultSettings = GD.Load<GameSettings>(DEFAULT_SETTINGS);
		WriteUserSettings(defaultSettings);
	}

	private void ReadUserSettings()
	{
		if (!FileAccess.FileExists(SETTINGS_FILE))
		{
			WriteDefaultUserSettings();
			return;
		}

		using var settingsFile = FileAccess.Open(
			SETTINGS_FILE,
			FileAccess.ModeFlags.Read
		);

		var s = new StringBuilder();
		while (settingsFile.GetPosition() < settingsFile.GetLength())
		{
			var line = settingsFile.GetLine();
			s.AppendLine(line);
		}

		GameSettings newSettings;
		try
		{
			newSettings =
				JsonConvert.DeserializeObject<GameSettings>(s.ToString())
				?? throw new Exception("Valid read, invalid settings file");
		}
		catch (Exception exception)
		{
			Logger.LogError(
				"Failed to read",
				ProjectSettings.GlobalizePath(SETTINGS_FILE),
				exception.ToString()
			);
			GameSettings = GD.Load<GameSettings>(DEFAULT_SETTINGS);
			return;
		}

		GameSettings = newSettings;
	}

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
