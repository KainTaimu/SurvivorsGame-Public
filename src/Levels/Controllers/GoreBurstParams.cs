// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace Game.Levels.Controllers;

[GlobalClass]
public partial class GoreBurstParams : Resource
{
	[Export]
	public int Count = 120;

	[ExportSubgroup("Velocity")]
	[Export]
	public float SpeedMin;

	[Export]
	public float SpeedMax = 600f;

	[ExportSubgroup("Scale")]
	[Export]
	public float ScaleMin = 0.1f;

	[Export]
	public float ScaleMax = 4f;

	[ExportSubgroup("Emission")]
	[Export]
	public float EmitRadius = 1f;

	[Export]
	public Vector2 EmitOffset = new(0, -8);

	[ExportSubgroup("Settle")]
	[Export]
	public float SettleMin = 0.02f;

	[Export]
	public float SettleMax = 0.16f;

	[ExportSubgroup("Direction")]
	[Export]
	public float SpreadDegrees = 38.5f;

	[ExportSubgroup("Color")]
	[Export]
	public Color BaseTint = new(0.7819659f, 0f, 0f);

	// Random per-particle hue shift added to BaseTint's hue.
	[Export]
	public float HueVariationMin = -0.12f;

	[Export]
	public float HueVariationMax = -0.05f;
}
