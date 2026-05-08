using Game.Levels.Controllers;

namespace Game.Items.Projectiles;

public interface IPooledProjectile
{
    ProjectilePool ProjectilePool { get; set; }
    void ReturnToPool();
}
