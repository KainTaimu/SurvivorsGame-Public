// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Items.Offensive;

[GlobalClass]
public partial class PumpActionStats : ItemStat
{
	[Export]
	public int PelletCount { get; set; } = 1;
}
