using System.Runtime.CompilerServices;
using Arch.Core;
using Game.Core.ECS;

namespace Game.Levels.Controllers;

public partial class EnemyMover : Node
{
	public override void _Process(double delta)
	{
		var player = GameWorld.Instance.MainPlayer;
		var playerPos = player.GlobalPosition;

		// var update = new MoveUpdate(new PositionComponent(playerPos));
		// GameWorld.World.InlineQuery<MoveUpdate, PositionComponent, MoveSpeedComponent>(
		// 	in new QueryDescription().WithAll<PositionComponent, MoveSpeedComponent>(),
		// 	ref update
		// );
		GameWorld.World.Query<PositionComponent, MoveSpeedComponent>(
			in new QueryDescription().WithAll<PositionComponent, MoveSpeedComponent>(),
			(ref pos, ref moveSpeed) =>
			{
				pos.Position = pos.Position.MoveToward(
					playerPos,
					(float)(moveSpeed.MoveSpeed * GameWorld.Instance.GetProcessDeltaTime())
				);
			}
		);
	}
}

public readonly struct MoveUpdate(PositionComponent moveToTarget) : IForEach<PositionComponent, MoveSpeedComponent>
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Update(ref PositionComponent pos, ref MoveSpeedComponent moveSpeed)
	{
		pos.Position.MoveToward(
			moveToTarget.Position,
			(float)(moveSpeed.MoveSpeed * GameWorld.Instance.GetProcessDeltaTime())
		);
	}
}
