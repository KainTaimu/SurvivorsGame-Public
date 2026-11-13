// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Newtonsoft.Json;

namespace Game.Core.Settings;

[GlobalClass]
[JsonObject(MemberSerialization.OptIn)]
public partial class GameSettings : Resource
{
	[ExportGroup("Sound")]
	[Export]
	[JsonProperty]
	public float MasterVolume
	{
		get;
		set
		{
			field = value;
			EmitSignalOnMasterVolumeChanged();
		}
	} = 0f;

	[ExportGroup("Game")]
	[Export]
	[JsonProperty]
	public float MouseSensitivity
	{
		get;
		set
		{
			field = value;
			EmitSignalOnMouseSensitivityChanged();
		}
	} = 1.0f;

	[Export]
	[JsonProperty]
	public bool EnableCameraShake
	{
		get;
		set
		{
			field = value;
			EmitSignalOnCameraShakeChanged();
		}
	} = true;

	[Export(PropertyHint.Range, "0,1,0.01")]
	[JsonProperty]
	public float CameraShakeScale
	{
		get;
		set
		{
			field = value;
			EmitSignalOnCameraShakeScaleChanged();
		}
	} = 1;

	[Export(PropertyHint.Range, "0.1,5,0.1")]
	[JsonProperty]
	public float CrosshairScale
	{
		get;
		set
		{
			field = value;
			EmitSignalOnCrosshairScaleChanged();
		}
	} = 1.5f;

	[Export]
	[JsonProperty]
	public bool EnableDamageIndicators
	{
		get;
		set
		{
			field = value;
			EmitSignalOnDamageIndicatorsChanged();
		}
	} = true;

	[ExportGroup("Graphics")]
	[Export]
	[JsonProperty]
	public bool EnableVSync
	{
		get;
		set
		{
			field = value;
			EmitSignalOnEnableVSyncChanged();
		}
	}

	[Export]
	[JsonProperty]
	public GoreEffectsEnum GoreEffects
	{
		get;
		set
		{
			field = value;
			EmitSignalOnGoreEffectsChanged();
		}
	} = GoreEffectsEnum.Medium;

	public int GoreEffectsValue => GoreEffectsConfigs.GoreEffectsMap[GoreEffects];

	[ExportGroup("DEV")]
	[Export]
	public bool EnableCrosshairHorizontalRecoilPunish = true;

	[Signal]
	public delegate void OnMasterVolumeChangedEventHandler();

	[Signal]
	public delegate void OnGoreEffectsChangedEventHandler();

	[Signal]
	public delegate void OnEnableVSyncChangedEventHandler();

	[Signal]
	public delegate void OnDamageIndicatorsChangedEventHandler();

	[Signal]
	public delegate void OnCameraShakeChangedEventHandler();

	[Signal]
	public delegate void OnCameraShakeScaleChangedEventHandler();

	[Signal]
	public delegate void OnCrosshairScaleChangedEventHandler();

	[Signal]
	public delegate void OnMouseSensitivityChangedEventHandler();

	public static GameSettings Instance = null!;

	public const string DEFAULT_SETTINGS = "uid://21pymi1puoaf";

	public static GameSettings GetDefaultGameSettings()
	{
		return GD.Load<GameSettings>(DEFAULT_SETTINGS);
	}
}
