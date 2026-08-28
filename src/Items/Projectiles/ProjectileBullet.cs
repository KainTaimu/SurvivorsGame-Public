using System.Collections.Generic;
using System.Linq;
using Arch.Core;
using Game.Core.ECS;
using Game.Core.Extensions;
using Game.Levels.Controllers;
using Game.Levels.Environment;

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

	private Viewport? _cachedViewport;
	private Camera2D? _cachedCamera;

	private static readonly PhysicsRayQueryParameters2D _cachedRayQuery = new();

	public override void _PhysicsProcess(double delta)
	{
		_cachedViewport ??= GetViewport();
		_cachedCamera ??= _cachedViewport.GetCamera2D();
		if (
			!_cachedViewport
				.GetVisibleRect()
				.GetCenteredToPoint(
					GameWorld.Instance.MainPlayer.GlobalPosition,
					1 / _cachedCamera.Zoom.GetLargestComponent()
				)
				.HasPoint(GlobalPosition)
		)
		{
			ReturnToPool();
			return;
		}

		if (!IsInitialized)
			Logger.LogWarning($"Projectile {GetType().Name} is processing but is not initialized");

		var from = Position;

		var moveVector = Vector2.Right.Rotated(Rotation) * ProjectileSpeed * (float)delta;
		_distanceTravelled += ProjectileSpeed * (float)delta;

		_cachedRayQuery.From = from;
		_cachedRayQuery.To = from + moveVector;
		_cachedRayQuery.CollideWithAreas = false;
		_cachedRayQuery.CollisionMask = 8u;

		var result = GetWorld2D().DirectSpaceState.IntersectRay(_cachedRayQuery);
		if (result.Count != 0)
		{
			var node = (Node)result["collider"];
			if (node is DestructableStaticBody2D destructableBody)
			{
				var destructable = (IDestructable)destructableBody.NodeOwner;
				// TODO: How to get damage from origin?
				destructable.TakeHit(10);
			}
			else
			{
				SpawnContactEffects((Vector2)result["position"], (Vector2)result["normal"]);
			}
			ReturnToPool();
			return;
		}

		Position = from + moveVector;

		foreach (var hit in _hits)
		{
			if (_distanceTravelled < hit.DistanceToHitPosition)
				continue;

			EmitSignalOnEntityHit(new EntityObject(hit.Target));
			_hitsHandled.Add(hit);
		}

		foreach (var hit in _hitsHandled)
			_hits.Remove(hit);

		if (_hitsHandled.Count >= PierceLimit)
		{
			Position = _hitsHandled.Last().Position;
			ReturnToPool();
		}
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

	private void SpawnContactEffects(Vector2 pos, Vector2 normal)
	{
		var data = new Godot.Collections.Dictionary<StringName, Variant>()
		{
			{ "position", pos },
			{ "rotation", normal.Angle() },
		};

		EnvironmentFxManager.PlayVfx("bullet_impact_concrete", data);
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
			if (!GameWorld.World.IsAlive(entity))
				continue;
			if (_hits.Any(data => data.Target == entity))
				continue;

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
