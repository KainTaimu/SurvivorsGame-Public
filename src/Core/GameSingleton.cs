// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Text;
using Game.Core.Settings;
using Newtonsoft.Json;

namespace Game.Core;

[GlobalClass]
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

	public override void _EnterTree()
	{
		Instance = this;
		ReadUserSettings();

		var debugMenu = GetNode("/root/DebugMenu");
		if (OS.HasFeature("prod"))
			debugMenu.QueueFree();
		else
			debugMenu.Set("style", 2); // Full with graph
	}

	public override void _ExitTree()
	{
		WriteUserSettings(GameSettings);
	}

	private void WriteUserSettings(GameSettings settings)
	{
		var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
		using var settingsFile = FileAccess.Open("user://settings.json", FileAccess.ModeFlags.Write);
		settingsFile.StoreLine(json);
	}

	private void WriteDefaultUserSettings()
	{
		var defaultSettings = GD.Load<GameSettings>(GameSettings.DEFAULT_SETTINGS);
		WriteUserSettings(defaultSettings);
	}

	private void ReadUserSettings()
	{
		if (!FileAccess.FileExists(SETTINGS_FILE))
		{
			WriteDefaultUserSettings();
			return;
		}

		using var settingsFile = FileAccess.Open(SETTINGS_FILE, FileAccess.ModeFlags.Read);

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
			CustomLogger.LogError("Failed to read", ProjectSettings.GlobalizePath(SETTINGS_FILE), exception.ToString());
			GameSettings = GD.Load<GameSettings>(GameSettings.DEFAULT_SETTINGS);
			return;
		}

		GameSettings = newSettings;
	}

	public override void _Input(InputEvent @event)
	{
		if (Input.IsPhysicalKeyPressed(Key.Quoteleft))
		{
			CustomLogger.LogDebug($"{Engine.GetFramesPerSecond()} FPS");
			GetTree().Quit();
			return;
		}

		if (Input.IsPhysicalKeyPressed(Key.F11))
		{
			CustomLogger.LogDebug($"Reloading current scene \"{GetTree().CurrentScene.Name}\"");
			GetTree().ReloadCurrentScene();
		}
	}
}
