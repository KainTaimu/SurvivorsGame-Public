// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Items.Offensive;

[GlobalClass]
public partial class PushOnAttackStats : ItemStat
{
	[Export]
	public float PushPerAttack { get; set; }
}
