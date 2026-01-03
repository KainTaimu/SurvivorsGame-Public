namespace Game.Core.ECS;

public struct PositionComponent(Vector2 pos)
{
    public required Vector2 Position = pos;
}

public struct MoveSpeedComponent(float speed)
{
    public required float MoveSpeed = speed;
}

public struct AnimatedSpriteComponent(string sprite, float animSpeed, int frameCount)
{
    public required string SpriteName = sprite;
    public required float AnimationSpeed = animSpeed;
    public required int FrameCount = frameCount;
    public int Frame = 0;
}

// Could be readonly
public struct CircleHitboxComponent(float radius)
{
    public required float Radius = radius;
}
