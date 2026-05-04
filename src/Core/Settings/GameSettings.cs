namespace Game.Core.Settings;

[GlobalClass]
public partial class GameSettings : Resource
{
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
}
