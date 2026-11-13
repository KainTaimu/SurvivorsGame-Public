// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Items.Offensive;

[GlobalClass]
public partial class ProjectileStats : ItemStat
{
	[Export]
	public int Speed { get; set; } = 3600;

	[Export]
	public int HitRadius { get; set; } = 32;

	[Export]
	public float Scale { get; set; } = 1f;
}
