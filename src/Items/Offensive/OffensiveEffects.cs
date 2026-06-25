using System.Runtime.CompilerServices;
using Arch.Core;
using Game.Core.ECS;
using Game.Levels.Controllers;

namespace Game.Items.Offensive;

public static class OffensiveEffects
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ApplyKnockback(in Entity entity, in Vector2 knockbackOrigin, float knockback)
	{
		if (!GameWorld.World.Has<PositionComponent>(entity))
			return;
		ref var pos = ref GameWorld.World.Get<PositionComponent>(entity);
		var knockbackVector = knockbackOrigin.DirectionTo(pos.Position);
		knockbackVector *= knockback;

		pos.Position += knockbackVector;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ApplyReduceVelocity(in Entity entity, float slowMultiplier = 1f)
	{
		if (!GameWorld.World.Has<VelocityComponent>(entity))
			return;
		ref var velocity = ref GameWorld.World.Get<VelocityComponent>(entity);
		velocity.Velocity *= slowMultiplier;
	}
}
