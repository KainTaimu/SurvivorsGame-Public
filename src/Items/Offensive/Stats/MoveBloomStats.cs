// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

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
	public double MoveTimeToMaxBloom { get; set; }

	[Export]
	public double MoveBloomGrowthRate { get; set; }

	[Export]
	public double MoveBloomShrinkRate { get; set; }

	[Export]
	public double MoveBloomMinDeg { get; set; }

	[Export]
	public double MoveBloomMaxDeg { get; set; }

	[Export]
	public int MoveDamageMin { get; set; }

	[Export]
	public int MoveDamageMax { get; set; }
}
