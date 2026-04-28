using System.Collections.Generic;
using Game.Core.ECS;
using Game.Items.Offensive;
using Game.Levels.Controllers;

namespace Game.Items.Projectiles;

// TODO:
// BaseWeapon should be responsible for handling damage, crit, etc
public partial class ProjectileBullet : BaseProjectile
{
	[Export]
	public float HitRadius = 24f;

	public float ProjectileSpeed;
	public int PierceLimit;

	private int _pierceCount;
	private readonly List<int> _hits = [];

	private BaseOffensive OffensiveOrigin => (BaseOffensive)Origin;
	private EnemyHitManager HitManager => EnemyHitManager.Instance;
	private EntityComponentStore ComponentStore =>
		EntityComponentStore.Instance;

	public override void _Ready()
	{
		var tweenSpeed = CreateTween()
			.BindNode(this)
			.SetTrans(Tween.TransitionType.Expo)
			.SetEase(Tween.EaseType.In);
		var tweenScale = CreateTween()
			.BindNode(this)
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
		tweenScale.TweenProperty(this, "scale", new Vector2(8, 1), 0.05);
	}

	public override void _Process(double delta)
	{
		var from = Position;

		var moveVector =
			new Vector2(1, 0).Rotated(Rotation)
			* ProjectileSpeed
			* (float)delta;
		var to = from + moveVector;

		Position = to;

		if (
			!HitManager.TryGetTargetAlongSegment(
				from,
				to,
				HitRadius,
				out var id
			)
		)
			return;

		if (_hits.Contains(id))
			return;

		ApplyDamage(id);
		ApplyKnockback(id);

		_hits.Add(id);
		_pierceCount++;
		if (_pierceCount >= PierceLimit)
			QueueFree();
	}

	private void ApplyDamage(int id)
	{
		if (!ComponentStore.GetComponent<HealthComponent>(id, out var health))
			return;

		var hit = new HitFeedbackComponent() { HitTime = 0.5f };
		if (!ComponentStore.GetComponent<HitFeedbackComponent>(id, out var _))
			ComponentStore.RegisterComponent(id, hit);
		else
			ComponentStore.SetComponent(id, hit);

		var crit = CritDamageCalculator.GetCritDamage(
			OffensiveOrigin.Stats.Damage,
			OffensiveOrigin.Stats.CritDamageMultiplier,
			OffensiveOrigin.Stats.CritChanceProportion
		);
		var newHealth = health.Health - OffensiveOrigin.Stats.Damage - crit;
		ComponentStore.SetComponent(id, health with { Health = newHealth });
	}

	private void ApplyKnockback(int id)
	{
		if (!ComponentStore.GetComponent<PositionComponent>(id, out var pos))
			return;
		var knockback = OffensiveOrigin
			.Stats.Additional.GetValueOrDefault("Knockback")
			.AsSingle();
		var knockbackVector =
			GameWorld.Instance.MainPlayer.GlobalPosition.DirectionTo(
				pos.Position
			);
		knockbackVector *= knockback;

		ComponentStore.SetComponent(
			id,
			pos with
			{
				Position = pos.Position + knockbackVector,
			}
		);
	}
}
