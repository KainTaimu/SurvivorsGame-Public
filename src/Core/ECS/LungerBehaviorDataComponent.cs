namespace Game.Core.ECS;

public record struct LungerBehaviorDataComponent(
	float TimeUntilNextLunge,
	float TimeBetweenLunges = 5,
	float MinDistanceToLunge = 250,
	float LungeVelocityMultiplier = 1.1f
);
