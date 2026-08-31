using Arch.Core;
using Game.Core.ECS;

namespace Game.Levels.Controllers;

[GlobalClass]
public partial class LungerEnemyBehavior : AbstractEnemyBehavior
{
	[Export]
	public float TimeBetweenLunges = 5;

	[Export]
	public float MinDistanceToLunge = 612;

	[Export]
	public float LungeVelocityMultiplier = 3f;

	public override void AddComponent(Entity entity)
	{
		GameWorld.World.Add(
			entity,
			new LungerBehaviorDataComponent(0, TimeBetweenLunges, MinDistanceToLunge, LungeVelocityMultiplier)
		);
	}
}
