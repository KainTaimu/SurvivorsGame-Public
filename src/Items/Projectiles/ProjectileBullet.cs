using System.Collections.Generic;
using Arch.Core;
using Game.Core.ECS;
using Game.Items.Offensive;
using Game.Levels.Controllers;

namespace Game.Items.Projectiles;

public partial class ProjectileBullet : BaseProjectile, IPooledProjectile
{
	[Export]
	public Sprite2D Sprite = null!;

	private int _pierceCount;
	private readonly List<Entity> _hits = [];

	private BaseOffensive OffensiveOrigin => (BaseOffensive)Origin;
	private EnemyTargetQuery TargetQuery => EnemyTargetQuery.Instance;

	public ProjectilePool ProjectilePool { get; set; } = null!;

	public override void _Process(double delta)
	{
		if (!TargetQuery.Grid.ContainsWorld(Position))
		{
			ReturnToPool();
			return;
		}

		if (!IsInitialized)
			Logger.LogWarning($"Projectile {GetType().Name} is processing but is not initialized");

		var from = Position;

		var moveVector = new Vector2(1, 0).Rotated(Rotation) * ProjectileSpeed * (float)delta;
		Position = from + moveVector;
	}

	public void ReturnToPool()
	{
		_pierceCount = 0;
		ProjectilePool.ReturnProjectile(this);
		IsInitialized = false;
	}

	public override void Initialize()
	{
		_hits.Clear();
		var tweenScale = CreateTween().SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.In);
		var finalScale = Scale * new Vector2(8, 1);
		tweenScale.TweenProperty(Sprite, "scale", finalScale, 0.05);
		IsInitialized = true;

		if (
			!TargetQuery.GetTargetsRayCast(
				Position,
				Rotation,
				HitRadius,
				out var hits,
				OffensiveOrigin.OffensiveStats.PierceLimit
			)
		)
			return;

		foreach (var entity in hits)
		{
			if (_hits.Contains(entity))
				return;

			var lastPos = GameWorld.World.Get<PositionComponent>(entity);

			// BUG:
			// URGENT
			// Because of the ray cast, the projectile may still hit an enemy that the projectile in front of
			// it has killed.
			Origin.GetTree().CreateTimer(Position.DistanceTo(lastPos.Position) / ProjectileSpeed, false).Timeout +=
				() =>
				{
					if (!GameWorld.World.IsAlive(entity))
						return;
					OffensiveOrigin.HandleHit(entity);
				};

			_hits.Add(entity);
			_pierceCount++;
			if (_pierceCount >= PierceLimit)
			{
				Origin.GetTree().CreateTimer(Position.DistanceTo(lastPos.Position) / ProjectileSpeed, false).Timeout +=
					ReturnToPool;
			}
		}
	}
}
