namespace Game.Core.ECS;

// Could be readonly
public struct CircleHitboxComponent(float radius)
{
    public required float Radius = radius;
}
