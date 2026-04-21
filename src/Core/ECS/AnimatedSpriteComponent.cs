namespace Game.Core.ECS;

public record struct AnimatedSpriteComponent(string sprite, float animSpeed, int frameCount)
{
	// TODO: Performance issue: LOTS of duplicate string objects in garbage collector. Enum?
	public required string SpriteName = sprite;
	public required float AnimationSpeed = animSpeed;
	public required int FrameCount = frameCount;
	public int Frame = 0;
}
