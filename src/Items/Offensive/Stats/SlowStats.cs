// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Items.Offensive;

[GlobalClass]
public partial class SlowStats : ItemStat
{
	[Export]
	public float SlowCoefficient { get; set; } = 1;
}
