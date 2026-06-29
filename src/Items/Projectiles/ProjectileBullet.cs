using System.Collections.Generic;
using System.Linq;
using Arch.Core;
using Game.Core.ECS;
using Game.Levels.Controllers;

namespace Game.Items.Projectiles;

public partial class ProjectileBullet : BaseProjectile, IPooledProjectile
{
	[Export]
	public Sprite2D Sprite = null!;

	private float _distanceTravelled;
	private int _pierceCount;
	private readonly List<HitData> _hits = [];
	private readonly List<HitData> _hitsHandled = [];

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

		var moveVector = Vector2.Right.Rotated(Rotation) * ProjectileSpeed * (float)delta;
		_distanceTravelled += ProjectileSpeed * (float)delta;
		Position = from + moveVector;

		_hitsHandled.Clear();
		foreach (var hit in _hits)
		{
			if (_distanceTravelled < hit.DistanceToHitPosition)
				continue;

			EmitSignalOnEntityHit(new EntityObject(hit.Target));
			_hitsHandled.Add(hit);
		}

		foreach (var hit in _hitsHandled)
		{
			_hits.Remove(hit);
		}

		if (_hitsHandled.Count == PierceLimit)
			ReturnToPool();
	}

	public void ReturnToPool()
	{
		_hits.Clear();
		_hitsHandled.Clear();
		_distanceTravelled = 0;
		_pierceCount = 0;
		ProjectilePool.ReturnProjectile(this);
		IsInitialized = false;
	}

	protected override void PostInitialization()
	{
		_hits.Clear();
		var tweenScale = CreateTween().SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.In);
		var finalScale = Scale * new Vector2(8, 1);
		tweenScale.TweenProperty(Sprite, "scale", finalScale, 0.05);
		IsInitialized = true;

		if (!TargetQuery.GetTargetsRayCast(Position, Rotation, HitRadius, out var hits, PierceLimit))
			return;

		foreach (var entity in hits)
		{
			// NOTE: Not sure if GetTargetsRayCast returns duplicates
			if (_hits.Any((data => data.Target == entity)))
				return;

			var entityPos = GameWorld.World.Get<PositionComponent>(entity);
			var distance = GlobalPosition.DistanceTo(entityPos.Position);

			_hits.Add(new HitData(entity, entityPos.Position, distance));
			_pierceCount++;
			if (_pierceCount >= PierceLimit)
				break;
		}
	}

	public readonly record struct HitData(Entity Target, Vector2 Position, float DistanceToHitPosition);
}
