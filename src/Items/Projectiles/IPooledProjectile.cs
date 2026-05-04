using Game.Levels.Controllers;

namespace Game.Items.Projectiles;

public interface IPooledProjectile
{
	public ProjectilePool ProjectilePool { get; set; }
	public void ReturnToPool();
}
