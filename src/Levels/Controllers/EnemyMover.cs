using System.Runtime.CompilerServices;
using Arch.System;
using Arch.System.SourceGenerator;
using Game.Core.ECS;

namespace Game.Levels.Controllers;

public partial class EnemyMover : Node
{
	public override void _Process(double delta)
	{
		var player = GameWorld.Instance.MainPlayer;
		var playerPos = player.GlobalPosition;

		MoveQuery(GameWorld.World, playerPos, (float)delta);
	}

	[Query(Parallel = true)]
	[All<PositionComponent, VelocityComponent, MoveSpeedComponent, CollisionLodComponent>]
	[None<DyingMarkerComponent>]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Move(
		[Data] in Vector2 moveToTarget,
		[Data] in float delta,
		ref PositionComponent pos,
		ref VelocityComponent velocity,
		ref MoveSpeedComponent moveSpeed,
		ref CollisionLodComponent lodLevel
	)
	{
		if (lodLevel.LodLevel < CollisionLodLevel.Far)
			return;
		velocity.Velocity = velocity.Velocity.Lerp(
			pos.Position.DirectionTo(moveToTarget) * moveSpeed.MoveSpeed,
			moveSpeed.TurnSpeed * delta
		);
		pos.Position += velocity.Velocity * delta;
	}
}
