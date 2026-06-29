using Arch.Core;
using Game.Core.ECS;
using Game.Core.Settings;
using Game.Levels.Controllers;
using Game.UI;

namespace Game.Items.Offensive;

public static class OffensiveEffects
{
	public static void ApplyDamage(
		Entity entity,
		int baseDamage,
		float crit,
		float randomDamageMult,
		float outgoingDamageMult
	)
	{
		if (!GameWorld.World.IsAlive(entity))
			return;

		if (!GameWorld.World.Has<HealthComponent>(entity))
			return;
		ref var health = ref GameWorld.World.Get<HealthComponent>(entity);

		var randomDamage =
			baseDamage > 1 ? Mathf.CeilToInt(GD.RandRange(-randomDamageMult, randomDamageMult) * baseDamage) : 0;
		var damage = Mathf.CeilToInt((baseDamage + crit + randomDamage) * outgoingDamageMult);
		health.Health -= damage;

		var hit = new HitFeedbackComponent
		{
			HitTime = 0.5f,
			Damage = damage,
			IsCrit = crit > 0,
		};
		GameWorld.World.Set(entity, hit);
	}

	public static void ApplyCameraShake(float shakeAmount, Func<Viewport> getViewport, Func<Tween> getTween)
	{
		if (!GameSettings.Instance.EnableCameraShake)
			return;

		var camera = getViewport().GetCamera2D();

		var origPos = camera.Position;
		var tween = getTween().SetTrans(Tween.TransitionType.Spring);

		for (var i = 0; i < 6; i++)
		{
			static int Rand()
			{
				return GD.RandRange(-1, 1);
			}

			var shake = new Vector2(Rand(), Rand()) * GD.RandRange(4, 9) * shakeAmount;

			tween.TweenProperty(camera, "offset", camera.Position + shake, 1 / 30f);
		}

		tween.TweenProperty(camera, "offset", origPos, 1 / 8f);
	}

	public static void ApplyCrosshairRecoil(
		Crosshair crosshair,
		float horizontalBaseRecoil,
		float horizontalRecoilMin,
		float horizontalRecoilRandom,
		float verticalBaseRecoil,
		float verticalRecoilMin,
		float verticalRecoilRandom,
		float recoilScale,
		float recoilAccumilationScale,
		bool applyHorizontalPunish = false
	)
	{
		var recoilX = (float)
			GD.Randfn(0, horizontalBaseRecoil + GD.RandRange(-horizontalRecoilRandom, horizontalRecoilRandom));
		recoilX = Math.Clamp(recoilX, -Math.Abs(horizontalRecoilMin), float.MaxValue);

		var recoilY = verticalBaseRecoil + Math.Abs((float)GD.Randfn(0, verticalRecoilRandom));
		recoilY = Math.Clamp(recoilY, verticalRecoilMin, float.MaxValue);

		var recoil = new Vector2(recoilX, -recoilY) * recoilScale;
		crosshair.Recoil.ApplyImpulse(recoil, recoilAccumilationScale, applyHorizontalPunish);
	}

	public static void ApplyKnockback(in Entity entity, in Vector2 awayFrom, float knockback)
	{
		if (!GameWorld.World.Has<PositionComponent>(entity))
			return;
		ref var pos = ref GameWorld.World.Get<PositionComponent>(entity);

		var knockbackVector = awayFrom.DirectionTo(pos.Position);
		knockbackVector *= knockback;

		pos.Position += knockbackVector;
	}

	public static void ApplyVelocityMultiplier(in Entity entity, float slowMultiplier = 1f)
	{
		if (!GameWorld.World.Has<VelocityComponent>(entity))
			return;
		ref var velocity = ref GameWorld.World.Get<VelocityComponent>(entity);

		velocity.Velocity *= slowMultiplier;
	}
}
