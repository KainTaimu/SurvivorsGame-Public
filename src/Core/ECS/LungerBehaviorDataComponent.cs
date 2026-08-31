namespace Game.Core.ECS;

public record struct LungerBehaviorDataComponent(
	float TimeUntilNextLunge,
	float TimeBetweenLunges = 5,
	float MinDistanceToLunge = 612,
	float LungeVelocityMultiplier = 1.1f
);
