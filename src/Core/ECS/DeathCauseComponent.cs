namespace Game.Core.ECS;

public enum DeathCauseEnum
{
	Normal,
	Explosion,
}

public readonly record struct DeathCauseComponent(DeathCauseEnum CauseEnum);
