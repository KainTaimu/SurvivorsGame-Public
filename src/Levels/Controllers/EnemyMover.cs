using System.Runtime.CompilerServices;
using Arch.Core;
using Game.Core.ECS;

namespace Game.Levels.Controllers;

public partial class EnemyMover : Node
{
	private static readonly QueryDescription _query = new QueryDescription()
		.WithAll<PositionComponent, VelocityComponent, MoveSpeedComponent>()
		.WithNone<DyingMarkerComponent>();

	public override void _Process(double delta)
	{
		var player = GameWorld.Instance.MainPlayer;
		var playerPos = player.GlobalPosition;

		var update = new MoveUpdate(new PositionComponent(playerPos), (float)GameWorld.Instance.GetProcessDeltaTime());
		GameWorld.World.InlineParallelQuery<MoveUpdate, PositionComponent, VelocityComponent, MoveSpeedComponent>(
			in _query,
			ref update
		);
	}
}

public readonly struct MoveUpdate(PositionComponent moveToTarget, float delta)
	: IForEach<PositionComponent, VelocityComponent, MoveSpeedComponent>
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Update(ref PositionComponent pos, ref VelocityComponent velocity, ref MoveSpeedComponent moveSpeed)
	{
		velocity.Velocity = pos.Position.DirectionTo(moveToTarget.Position) * moveSpeed.MoveSpeed;
		pos.Position += velocity.Velocity * delta;
	}
}
