namespace Game.Items.Projectiles;

public abstract partial class BaseProjectile : Node2D
{
	public BaseItem Origin = null!;

	public float HitRadius = 24f;
	public float ProjectileSpeed;
	public int PierceLimit;
}
