using Arch.Core;

namespace Game.Items.Projectiles;

// need this because c# structs aren't supported in signals
public partial class EntityObject(Entity entity) : RefCounted
{
	public readonly Entity Entity = entity;
}
