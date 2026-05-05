using Newtonsoft.Json;

namespace Game.Core.Settings;

[GlobalClass]
[JsonObject(MemberSerialization.OptIn)]
public partial class GameSettings : Resource
{
	[ExportGroup("Game")]
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
	public GoreEffectsEnum GoreEffects
	{
		get;
		set
		{
			field = value;
			EmitSignalOnGoreEffectsChanged();
		}
	} = GoreEffectsEnum.Medium;
	public int GoreEffectsValue =>
		GoreEffectsConfigs.GoreEffectsMap[GoreEffects];

	[ExportGroup("DEV")]
	[Export]
	public bool EnableCrosshairHorizontalRecoilPunish = true;

	[Signal]
	public delegate void OnGoreEffectsChangedEventHandler();

	[Signal]
	public delegate void OnDamageIndicatorsChangedEventHandler();

	[Signal]
	public delegate void OnCameraShakeChangedEventHandler();

	[Signal]
	public delegate void OnCameraShakeScaleChangedEventHandler();

	[Signal]
	public delegate void OnCrosshairScaleChangedEventHandler();

	public static GameSettings Instance = null!;
}
