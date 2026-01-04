namespace Game.Core.ECS;

public struct PositionComponent(Vector2 pos)
{
    public required Vector2 Position = pos;
}
