// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Items.Offensive;

[GlobalClass]
public partial class AttackStats : ItemStat
{
	[Export]
	public float AttackSpeed { get; set; } = 0.2f;
}
