// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace Game.Items.Offensive;

[GlobalClass]
public partial class DamageStats : ItemStat
{
	[Export]
	public int Damage { get; set; } = 4;

	[Export]
	public float DamageVariance { get; set; } = 0.15f;

	[Export]
	public float CritDamageMultiplier { get; set; } = 1.5f;

	[Export(PropertyHint.Range, "0,1,0.01")]
	public float CritDamageChance { get; set; } = 0.1f;

	public int CalculateCrit()
	{
		var roll = GD.Randf();
		if (roll > CritDamageChance)
			return 0;

		return Mathf.CeilToInt(Damage * CritDamageMultiplier);
	}
}
