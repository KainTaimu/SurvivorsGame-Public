using System.Collections.Generic;
using Game.Core.ECS;
using Game.Items.Offensive;
using Game.Levels.Controllers;

namespace Game.Items.Projectiles;

public partial class ProjectileBullet : BaseProjectile, IPooledProjectile
{
	[Export]
	public Sprite2D Sprite = null!;

	private int _pierceCount;
	private readonly List<int> _hits = [];

	private BaseOffensive OffensiveOrigin => (BaseOffensive)Origin;
	private EnemyTargetQuery TargetQuery => EnemyTargetQuery.Instance;

	private EntityComponentStore ComponentStore =>
		EntityComponentStore.Instance;

	public ProjectilePool ProjectilePool { get; set; } = null!;

	public override void _Process(double delta)
	{
		if (!TargetQuery.Grid.ContainsWorld(Position))
		{
			ReturnToPool();
			return;
		}

		if (!IsInitialized)
		{
			Logger.LogWarning(
				$"Projectile {GetType().Name} is processing but is not initialized"
			);
		}

		var from = Position;

		var moveVector =
			new Vector2(1, 0).Rotated(Rotation)
			* ProjectileSpeed
			* (float)delta;
		Position = from + moveVector;
	}

	public void ReturnToPool()
	{
		_pierceCount = 0;
		_hits.Clear();
		ProjectilePool.ReturnProjectile(this);
		IsInitialized = false;
	}

	public override void Initialize()
	{
		var tweenScale = CreateTween()
			.SetTrans(Tween.TransitionType.Linear)
			.SetEase(Tween.EaseType.In);
		var finalScale = Scale * new Vector2(8, 1);
		tweenScale.TweenProperty(Sprite, "scale", finalScale, 0.05);
		IsInitialized = true;

		if (
			!TargetQuery.GetTargetsRayCast(
				Position,
				Rotation,
				HitRadius,
				out var ids
			)
		)
			return;

		foreach (var id in ids)
		{
			if (_hits.Contains(id))
				continue;

			if (
				!ComponentStore.GetComponent<PositionComponent>(
					id,
					out var lastPos
				)
			)
				continue;

			Callable
				.From(() =>
				{
					GetTree()
						.CreateTimer(
							Position.DistanceTo(lastPos.Position)
								/ ProjectileSpeed,
							false
						)
						.Timeout += () => OffensiveOrigin.HandleHit(id: id);
				})
				.CallDeferred();

			_hits.Add(id);
			_pierceCount++;
			if (_pierceCount >= PierceLimit)
			{
				Callable
					.From(() =>
					{
						GetTree()
							.CreateTimer(
								Position.DistanceTo(lastPos.Position)
									/ ProjectileSpeed,
								false
							)
							.Timeout += () =>
							Callable.From(ReturnToPool).CallDeferred();
					})
					.CallDeferred();
				return;
			}
		}
	}
}
