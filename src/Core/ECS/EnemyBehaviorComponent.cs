using Game.Levels.Controllers;

namespace Game.Core.ECS;

public readonly record struct EnemyBehaviorComponent(EnemyBehaviorTypes Type = EnemyBehaviorTypes.None);
