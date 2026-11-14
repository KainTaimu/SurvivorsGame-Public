// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Items.Offensive;

[GlobalClass]
public partial class MoveBloomStats : ItemStat
{
	public double MoveTime
	{
		get;
		set => field = Math.Clamp(value, 0, MoveTimeToMaxBloom);
	}

	public double MoveTimeFactor => MoveTime / (MoveTimeToMaxBloom != 0 ? MoveTimeToMaxBloom : 1 / double.MaxValue);

	[Export]
	public double MoveTimeToMaxBloom { get; set; } = 0.2;

	[Export]
	public double MoveBloomGrowthRate { get; set; } = 1;

	[Export]
	public double MoveBloomShrinkRate { get; set; } = 1.2;

	[Export]
	public double MoveBloomPerfectBloomLimit { get; set; } = 0.05;

	/// <summary>
	/// Gets or sets the bloom angle, in degrees, used when movement-based bloom is at its perfect accuracy state.
	/// </summary>
	[Export]
	public double MoveBloomPerfectBloomDeg { get; set; }

	/// <summary>
	/// Gets or sets the minimum bloom angle, in degrees, applied when movement-based bloom is calculated.
	/// </summary>
	[Export]
	public double MoveBloomMinDeg { get; set; }

	/// <summary>
	/// Gets or sets the maximum bloom angle, in degrees, applied when movement-based bloom reaches its upper limit.
	/// </summary>
	[Export]
	public double MoveBloomMaxDeg { get; set; } = 10;

	[Export]
	public int MoveDamageMin { get; set; }

	[Export]
	public int MoveDamageMax { get; set; }

	public bool IsPerfectBloom => MoveTimeFactor < MoveBloomPerfectBloomLimit;
}
