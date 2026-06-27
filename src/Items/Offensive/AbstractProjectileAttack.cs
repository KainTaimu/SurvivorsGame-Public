using Game.Items.Projectiles;

namespace Game.Items.Offensive;

[GlobalClass]
public abstract partial class AbstractProjectileAttack : Resource, IProjectileAttack
{
	public abstract BaseProjectile Attack(
		Func<BaseProjectile> getProjectile,
		Vector2 origin,
		float rotation,
		float radius,
		float speed,
		int pierceLimit,
		Vector2 spriteScale
	);
}
