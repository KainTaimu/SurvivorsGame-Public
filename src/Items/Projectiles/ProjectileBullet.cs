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

	public override void _Ready()
	{
		var tweenSpeed = CreateTween()
			.SetTrans(Tween.TransitionType.Expo)
			.SetEase(Tween.EaseType.In);
		var tweenScale = CreateTween()
			.SetTrans(Tween.TransitionType.Linear)
			.SetEase(Tween.EaseType.In);
		var originalSpeed = ProjectileSpeed;

		ProjectileSpeed = originalSpeed / 3;
		tweenSpeed.TweenProperty(
			this,
			nameof(ProjectileSpeed),
			originalSpeed,
			0.13f
		);

		var finalScale = Scale * new Vector2(8, 1);
		tweenScale.TweenProperty(Sprite, "scale", finalScale, 0.05);
	}

	public override void _Process(double delta)
	{
		if (!TargetQuery.Grid.ContainsWorld(Position))
		{
			ReturnToPool();
			return;
		}

		var from = Position;

		var moveVector =
			new Vector2(1, 0).Rotated(Rotation)
			* ProjectileSpeed
			* (float)delta;
		Position = from + moveVector;

		if (
			!TargetQuery.TryGetTargetsInAreaAlongSegment(
				from,
				Position,
				HitRadius,
				out var ids
			)
		)
			return;

		foreach (var id in ids)
		{
			if (_hits.Contains(id))
				return;

			OffensiveOrigin.HandleHit(id: id);

			_hits.Add(id);
			_pierceCount++;
			if (_pierceCount >= PierceLimit)
			{
				Callable.From(ReturnToPool).CallDeferred();
				return;
			}
		}
	}

	public void ReturnToPool()
	{
		_pierceCount = 0;
		_hits.Clear();
		ProjectilePool.ReturnProjectile(this);
	}
}
