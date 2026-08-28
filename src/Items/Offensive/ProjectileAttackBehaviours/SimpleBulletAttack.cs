using Game.Items.Projectiles;

namespace Game.Items.Offensive;

[GlobalClass]
public partial class SimpleBulletAttack : AbstractProjectileAttack
{
	public override BaseProjectile Attack(
		Func<BaseProjectile> getProjectile,
		Vector2 origin,
		float rotation,
		float radius,
		float speed,
		int pierceLimit,
		Vector2 spriteScale
	)
	{
		var projectile = getProjectile();

		projectile.SetScale(spriteScale);
		projectile.SetPosition(origin);
		projectile.Rotation = rotation;

		projectile.Initialize(radius, speed, pierceLimit);
		return projectile;
	}
}
