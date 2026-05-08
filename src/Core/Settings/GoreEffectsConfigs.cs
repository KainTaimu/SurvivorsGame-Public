using System.Collections.Generic;

namespace Game.Core.Settings;

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

public enum GoreEffectsEnum
{
	Disabled,
	Low,
	Medium,
	High,
}
