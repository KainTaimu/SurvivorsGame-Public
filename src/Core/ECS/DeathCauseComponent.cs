namespace Game.Core.ECS;

public enum DeathCauseEnum
{
	Normal,
	Explosion,
	LargeDeath,
}

public readonly record struct DeathCauseComponent(DeathCauseEnum CauseEnum);
