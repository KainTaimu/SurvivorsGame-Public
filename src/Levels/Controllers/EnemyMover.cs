using System.Runtime.CompilerServices;
using Arch.System;
using Arch.System.SourceGenerator;
using Game.Core.ECS;

namespace Game.Levels.Controllers;

public partial class EnemyMover : Node
{
	[Export]
	public float VelocityRecoveryFactor = 5f;

	public override void _Process(double delta)
	{
		var player = GameWorld.Instance.MainPlayer;
		var playerPos = player.GlobalPosition;

		MoveQuery(GameWorld.World, playerPos, VelocityRecoveryFactor, (float)delta);
	}

	[Query(Parallel = true)]
	[All<PositionComponent, VelocityComponent, MoveSpeedComponent>]
	[None<DyingMarkerComponent>]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Move(
		[Data] in Vector2 moveToTarget,
		[Data] in float recoveryRate,
		[Data] in float delta,
		ref PositionComponent pos,
		ref VelocityComponent velocity,
		ref MoveSpeedComponent moveSpeed
	)
	{
		velocity.Velocity = velocity.Velocity.Lerp(
			pos.Position.DirectionTo(moveToTarget) * moveSpeed.MoveSpeed,
			recoveryRate * delta
		);
		pos.Position += velocity.Velocity * delta;
	}
}
