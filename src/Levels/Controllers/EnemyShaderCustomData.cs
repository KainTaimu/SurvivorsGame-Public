// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace Game.Levels.Controllers;

public readonly struct EnemyShaderCustomData(
	byte frameX,
	byte frameY,
	byte frameSizePxX,
	byte frameSizePxY,
	byte frameIdxX = 0,
	byte frameIdxY = 0,
	bool flip = false,
	byte opacity = 255,
	byte flash = 0,
	byte scale = 1
)
{
	public float R => GetR();
	public float G => GetG();
	public float B => GetB();
	public float A => GetA();

	// float has 32 bits
	// Channel R | 17 bits
	public readonly bool Flip = flip; // 1 bit
	public readonly byte Opacity = opacity; // 1 byte 8 bits
	public readonly byte Flash = flash; // 1 byte 8 bits
	private const int FLIP_POSITION = 0;
	private const int OPACITY_POSITION = 1;
	private const int FLASH_POSITION = 9;

	// Channel B | 4 bytes 32 bits
	public readonly byte FrameX = frameX;
	public readonly byte FrameY = frameY;
	public readonly byte FrameIdxX = frameIdxX;
	public readonly byte FrameIdxY = frameIdxY;
	private const int FRAME_X_POSITION = 0;
	private const int FRAME_Y_POSITION = 8;
	private const int FRAME_IDX_X_POSITION = 16;
	private const int FRAME_IDX_Y_POSITION = 24;

	// Channel A | 3 bytes 24 bits
	public readonly byte FrameSizePxX = frameSizePxX;
	public readonly byte FrameSizePxY = frameSizePxY;
	public readonly byte Scale = scale;
	private const int FRAME_SIZE_PX_X_POSITION = 0;
	private const int FRAME_SIZE_PX_Y_POSITION = 8;
	private const int SCALE_X_POSITION = 16;

	private float GetR()
	{
		var bits = 0u;

		bits ^= (Flip ? 1u : 0u) << FLIP_POSITION;
		bits ^= (uint)Opacity << OPACITY_POSITION;
		bits ^= (uint)Flash << FLASH_POSITION;

		return BitConverter.UInt32BitsToSingle(bits);
	}

	// TODO: What to use channel G for
	private float GetG()
	{
		return 0f;
	}

	private float GetB()
	{
		var bits = 0u;

		bits ^= (uint)FrameX << FRAME_X_POSITION;
		bits ^= (uint)FrameY << FRAME_Y_POSITION;
		bits ^= (uint)FrameIdxX << FRAME_IDX_X_POSITION;
		bits ^= (uint)FrameIdxY << FRAME_IDX_Y_POSITION;

		return BitConverter.UInt32BitsToSingle(bits);
	}

	private float GetA()
	{
		var bits = 0u;

		bits ^= (uint)FrameSizePxX << FRAME_SIZE_PX_X_POSITION;
		bits ^= (uint)FrameSizePxY << FRAME_SIZE_PX_Y_POSITION;
		bits ^= (uint)Scale << SCALE_X_POSITION;

		return BitConverter.UInt32BitsToSingle(bits);
	}
}
