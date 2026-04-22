namespace Game.Core.ECS;

public record struct AnimatedSpriteComponent(
	string sprite,
	float animSpeed,
	byte frameCountX,
	byte frameCountY,
	byte frameSizePxX,
	byte frameSizePxY,
	byte frameIdxX = 0,
	byte frameIdxY = 0
)
{
	// TODO: Performance issue: LOTS of duplicate string objects in garbage collector. Enum?
	public required string SpriteName = sprite;
	public required float AnimationSpeed = animSpeed;
	public required byte FrameCountX = frameCountX;
	public required byte FrameCountY = frameCountY;
	public required byte FrameSizePxX = frameSizePxX;
	public required byte FrameSizePxY = frameSizePxY;
	public byte FrameIdxX = frameIdxX;
	public byte FrameIdxY = frameIdxY;
	public double AnimationTime = animSpeed;
}
