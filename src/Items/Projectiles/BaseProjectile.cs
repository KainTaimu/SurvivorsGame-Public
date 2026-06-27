namespace Game.Items.Projectiles;

public abstract partial class BaseProjectile : Node2D, IProjectile
{
	[Signal]
	public delegate void OnEntityHitEventHandler(EntityObject entityObject);

	public float HitRadius { get; private set; } = 24f;
	public float ProjectileSpeed { get; private set; }
	public int PierceLimit { get; private set; }

	protected bool IsInitialized { get; set; }

	public void Initialize(float hitRadius, float speed, int pierceLimit)
	{
		HitRadius = hitRadius;
		ProjectileSpeed = speed;
		PierceLimit = pierceLimit;
		IsInitialized = true;
		PostInitialization();
	}

	protected abstract void PostInitialization();
}
