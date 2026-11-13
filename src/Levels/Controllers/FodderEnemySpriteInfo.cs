// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace Game.Levels.Controllers;

[GlobalClass]
public partial class FodderEnemySpriteInfo : Resource
{
	[Export]
	public Texture2D SpriteFrames = null!;

	[Export]
	public string SpriteName = null!;

	[Export]
	public float AnimationSpeed = 0.5f;

	[Export]
	public byte FrameCountX = 1;

	[Export]
	public byte FrameSizePxX = 40;

	[Export]
	public byte FrameSizePxY = 40;

	[Export]
	public byte Opacity = 255;

	[Export]
	public byte Flash;
}
