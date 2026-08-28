using Game.Items.Projectiles;

namespace Game.Items.Offensive;

public interface IProjectileAttack
{
	BaseProjectile Attack(
		Func<BaseProjectile> getProjectile,
		Vector2 origin,
		float rotation,
		float radius,
		float speed,
		int pierceLimit,
		Vector2 spriteScale
	);
}
