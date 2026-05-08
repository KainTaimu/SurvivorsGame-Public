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
    private const int FLIP_POSITION = 0;
    private const int OPACITY_POSITION = 1;
    private const int FLASH_POSITION = 9;

    // Channel B
    public readonly byte FrameX = frameX;
    public readonly byte FrameY = frameY;
    public readonly byte FrameIdxX = frameIdxX;
    public readonly byte FrameIdxY = frameIdxY;
    private const int FRAME_X_POSITION = 0;
    private const int FRAME_Y_POSITION = 8;
    private const int FRAME_IDX_X_POSITION = 16;
    private const int FRAME_IDX_Y_POSITION = 24;

    // Channel A
    public readonly byte FrameSizePxX = frameSizePxX;
    public readonly byte FrameSizePxY = frameSizePxY;
    private const int FRAME_SIZE_PX_X_POSITION = 0;
    private const int FRAME_SIZE_PX_Y_POSITION = 8;

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

        return BitConverter.UInt32BitsToSingle(bits);
    }
}
