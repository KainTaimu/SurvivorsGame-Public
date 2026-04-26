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
	byte Flash = 0
)
{
	public double AnimationTime { get; set; } = AnimationSpeed;
}
