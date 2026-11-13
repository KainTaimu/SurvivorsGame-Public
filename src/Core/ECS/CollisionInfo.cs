namespace Game.Core.ECS;

public enum CollisionLodLevel
{
	None,
	Near,
	Far,
}

public record struct CollisionLodComponent(CollisionLodLevel LodLevel);
