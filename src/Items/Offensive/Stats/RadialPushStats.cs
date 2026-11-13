// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Items.Offensive;

[GlobalClass]
public partial class RadialPushStats : ItemStat
{
	[Export]
	public float NearbyPushRadius { get; set; }

	[Export]
	public float NearbyPushAmount { get; set; }
}
