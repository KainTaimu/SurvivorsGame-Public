// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Items.Offensive;

[GlobalClass]
public partial class GrenadeStats : ItemStat
{
	[Export]
	public float TimeToExplode { get; set; }

	[Export]
	public int EnemiesInAreaUntilExplode { get; set; }

	[Export]
	public float ArmingDistance { get; set; }
}
