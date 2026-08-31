using Arch.Core;

namespace Game.Levels.Controllers;

[GlobalClass]
public abstract partial class AbstractEnemyBehavior : Resource
{
	public abstract void AddComponent(Entity entity);
}
