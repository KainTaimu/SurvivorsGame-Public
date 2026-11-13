using System.Collections.Generic;

namespace Game.Core.Settings;

public static class GoreEffectsConfigs
{
	public static readonly Dictionary<GoreEffectsEnum, int> GoreEffectsMap = new()
	{
		{ GoreEffectsEnum.Disabled, 0 },
		{ GoreEffectsEnum.Low, 1000 },
		{ GoreEffectsEnum.Medium, 2500 },
		{ GoreEffectsEnum.High, 5000 },
		{ GoreEffectsEnum.VeryHigh, 10_000 },
	};
}

public enum GoreEffectsEnum
{
	Disabled,
	Low,
	Medium,
	High,
	VeryHigh,
}
