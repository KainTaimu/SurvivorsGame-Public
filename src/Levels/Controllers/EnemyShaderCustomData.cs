namespace Game.Levels.Controllers;

public class EnemyShaderCustomData(
	byte frameX,
	byte frameY,
	byte frameSizePxX,
	byte frameSizePxY,
	byte frameIdxX = 0,
	byte frameIdxY = 0,
	bool flip = false,
	byte opacity = 255,
	byte flash = 0
)
{
	public float R => GetR();
	public float G => GetG();
	public float B => GetB();
	public float A => GetA();

	// Channel R
	public bool Flip = flip; // 1 bit
	public byte Opacity = opacity; // 2 byte = 8 bit
	public byte Flash = flash;
	private const int FlipPosition = 0;
	private const int OpacityPosition = 1;
	private const int FlashPosition = 9;

	// Channel B
	public readonly byte FrameX = frameX;
	public readonly byte FrameY = frameY;
	public readonly byte FrameIdxX = frameIdxX;
	public readonly byte FrameIdxY = frameIdxY;
	private const int FrameXPosition = 0;
	private const int FrameYPosition = 8;
	private const int FrameIdxXPosition = 16;
	private const int FrameIdxYPosition = 24;

	// Channel A
	public readonly byte FrameSizePxX = frameSizePxX;
	public readonly byte FrameSizePxY = frameSizePxY;
	private const int FrameSizePxXPosition = 0;
	private const int FrameSizePxYPosition = 8;

	private float GetR()
	{
		var bits = 0u;

		bits ^= (Flip ? 1u : 0u) << FlipPosition;
		bits ^= (uint)Opacity << OpacityPosition;
		bits ^= (uint)Flash << FlashPosition;

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

		bits ^= (uint)FrameX << FrameXPosition;
		bits ^= (uint)FrameY << FrameYPosition;
		bits ^= (uint)FrameIdxX << FrameIdxXPosition;
		bits ^= (uint)FrameIdxY << FrameIdxYPosition;

		return BitConverter.UInt32BitsToSingle(bits);
	}

	private float GetA()
	{
		var bits = 0u;

		bits ^= (uint)FrameSizePxX << FrameSizePxXPosition;
		bits ^= (uint)FrameSizePxY << FrameSizePxYPosition;

		return BitConverter.UInt32BitsToSingle(bits);
	}
}

