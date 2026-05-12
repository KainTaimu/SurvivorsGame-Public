using Game.Core.Settings;

namespace Game.Core;

public partial class AudioManager : Node
{
	public float MasterVolume
	{
		get => GameSettings.Instance.MasterVolume;
		set => UpdateMasterBusVolume();
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

		AudioServer.SetBusVolumeDb(masterBusIndex, MasterVolume);
		Logger.LogDebug($"Updated master bus volume to {AudioServer.GetBusVolumeDb(masterBusIndex)} dB");
		Logger.LogDebug($"Master volume: {MasterVolume}");
	}
}
