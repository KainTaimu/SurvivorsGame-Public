// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Levels.Controllers;

[Flags]
public enum CardinalDirection
{
	None = 0,
	North = 1 << 1,
	West = 1 << 2,
	East = 1 << 3,
	South = 1 << 4,
}
