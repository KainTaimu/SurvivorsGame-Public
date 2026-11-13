// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace Game.Enemies;

[GlobalClass]
public partial class EnemyStats : Resource
{
	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public int DamageOnContact = 2;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public int MaxHealth = 5;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public float MoveSpeed = 180;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public float TurnSpeed = 5;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public int Defense;

	[Export(PropertyHint.Range, "0.5,5,or_greater,hide_slider")]
	public float SpriteScaleMultiplier = 1;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public int MoneyDrop = 10;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public int XpDrop = 10;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public int HealthRegenPerSecond;

	[ExportCategory("Multiplier attributes")]
	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public float HealthMultiplier = 1;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public float MoveSpeedMultiplier = 1;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public float ContactDamageMultiplier = 1;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public float OutgoingDamageMultiplier = 1;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public float XpDropMultiplier = 1;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public float MoneyDropMultiplier = 1;
}
