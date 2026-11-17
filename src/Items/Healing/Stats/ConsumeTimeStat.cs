// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Items.Healing;

[GlobalClass]
public partial class ConsumeTimeStat : ItemStat
{
	[Export]
	public float TimeToConsume { get; set; } = 1;
}

public interface IHasConsumeTimeStat
{
	ConsumeTimeStat ConsumeTimeStat { get; }
}
