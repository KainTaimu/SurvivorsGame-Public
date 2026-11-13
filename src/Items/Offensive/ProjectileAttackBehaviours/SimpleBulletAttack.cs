using Game.Items.Projectiles;

namespace Game.Items.Offensive;

[GlobalClass]
public partial class SimpleBulletAttack : AbstractProjectileAttack
{
	public override BaseProjectile Attack(
		Func<BaseProjectile> getProjectile,
		Node? origin,
		Vector2 position,
		float rotation,
		float radius,
		float speed,
		int pierceLimit,
		Vector2 spriteScale
	)
	{
		var projectile = getProjectile();

		projectile.SetScale(spriteScale);
		projectile.SetPosition(position);
		projectile.Rotation = rotation;

		projectile.Initialize(origin, radius, speed, pierceLimit);
		return projectile;
	}
}
