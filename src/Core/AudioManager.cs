// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Game.Core.Settings;

namespace Game.Core;

public partial class AudioManager : Node
{
	private float _masterVolume;

	public float MasterVolume
	{
		get => GameSettings.Instance.MasterVolume;
		set
		{
			_masterVolume = value;
			UpdateMasterBusVolume();
		}
	}

	public override void _Ready()
	{
		GameSettings.Instance.OnMasterVolumeChanged += () =>
		{
			MasterVolume = GameSettings.Instance.MasterVolume;
		};
		MasterVolume = GameSettings.Instance.MasterVolume;
	}

	private void UpdateMasterBusVolume()
	{
		var masterBusIndex = AudioServer.GetBusIndex("Master");

		AudioServer.SetBusVolumeLinear(masterBusIndex, MasterVolume);
		CustomLogger.LogDebug($"Updated master bus volume to {AudioServer.GetBusVolumeDb(masterBusIndex)} dB");
		CustomLogger.LogDebug($"Master volume: {MasterVolume}");
	}
}
