namespace Game.Core.ECS;

public readonly struct EntityTypeComponent(EntityType type)
{
    public readonly EntityType EntityType = type;
}
