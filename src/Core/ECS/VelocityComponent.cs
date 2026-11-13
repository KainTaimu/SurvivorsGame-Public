namespace Game.Core.ECS;

public struct VelocityComponent(Vector2 velocity)
{
	public Vector2 Velocity = velocity;
	public static readonly VelocityComponent Zero = new(Vector2.Zero);
}
