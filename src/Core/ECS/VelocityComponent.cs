namespace Game.Core.ECS;

public record struct VelocityComponent(Vector2 Velocity)
{
	public static readonly VelocityComponent Zero = new(Vector2.Zero);
}
