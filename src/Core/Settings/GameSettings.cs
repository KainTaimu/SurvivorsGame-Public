namespace Game.Core.Settings;

[GlobalClass]
public partial class GameSettings : Resource
{
	[ExportGroup("Game")]
	[Export]
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
}
