using System.Collections.Generic;

namespace Game.Core;

public enum GoreEffectsEnum
{
	Disabled,
	Low,
	Medium,
	High,
}

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

public static class GoreEffectsConfigs
{
	public static readonly Dictionary<GoreEffectsEnum, int> GoreEffectsMap =
		new()
		{
			{ GoreEffectsEnum.Disabled, 0 },
			{ GoreEffectsEnum.Low, 200 },
			{ GoreEffectsEnum.Medium, 800 },
			{ GoreEffectsEnum.High, 1200 },
		};
}
