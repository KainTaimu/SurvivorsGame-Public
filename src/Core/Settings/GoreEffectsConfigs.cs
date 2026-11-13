// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

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
