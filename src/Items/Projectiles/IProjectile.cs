namespace Game.Items.Projectiles;

public interface IProjectile
{
	float HitRadius { get; }
	float ProjectileSpeed { get; }
	int PierceLimit { get; }
}
