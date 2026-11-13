// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace Game.Core.ECS;

public record struct AnimatedSpriteComponent(
	string SpriteName,
	float AnimationSpeed,
	byte FrameCountX,
	byte FrameCountY,
	byte FrameSizePxX,
	byte FrameSizePxY,
	byte FrameIdxX = 0,
	byte FrameIdxY = 0,
	byte Opacity = 25,
	byte Flash = 0,
	float Scale = 1
)
{
	public double AnimationTime { get; set; } = AnimationSpeed;
}
