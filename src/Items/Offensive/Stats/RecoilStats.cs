// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace Game.Items.Offensive;

[GlobalClass]
public partial class RecoilStats : ItemStat
{
	[Export]
	public float HorizontalRecoilMin { get; set; } = 1f;

	[Export]
	public float HorizontalBaseRecoil { get; set; } = 3f;

	[Export]
	public float HorizontalRecoilRandom { get; set; } = 1f;

	[Export]
	public float VerticalRecoilMin { get; set; } = 2f;

	[Export]
	public float VerticalBaseRecoil { get; set; } = 3f;

	[Export]
	public float VerticalRecoilRandom { get; set; } = 0.1f;

	[Export]
	public float RecoilScale { get; set; } = 1f;

	[Export]
	public float RecoilAccumilationScale { get; set; } = 1f;

	[Export]
	public bool HorizontalRecoilPunish { get; set; } = true;

	[Export]
	public float CameraRecoilScale { get; set; } = 1f;
}
