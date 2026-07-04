using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Arch.System;
using Arch.System.SourceGenerator;
using Game.Core.ECS;

namespace Game.Levels.Controllers;

public partial class EnemyNavMeshMover : Node2D
{
	[Export]
	public bool DrawNavPaths;

	private readonly ConcurrentQueue<Vector2[]> _lines = [];

	private static Vector2 _playerPosition;

	public override void _Process(double delta)
	{
		var player = GameWorld.Instance.MainPlayer;
		_playerPosition = player.GlobalPosition;

		UpdateMoversQuery(GameWorld.World, NavMap.Instance.GridVisibilityRect, (float)delta, DrawNavPaths, _lines);

#if DEBUG
		if (DrawNavPaths && Engine.GetProcessFrames() % 20 == 0)
			QueueRedraw();
#endif
	}

	public override void _Draw()
	{
		while (_lines.TryDequeue(out var line))
			DrawPolyline(line, Colors.Red, 1);
		_lines.Clear();
	}

	[Query(Parallel = true)]
	[All<PositionComponent, VelocityComponent, MoveSpeedComponent>]
	[None<DyingMarkerComponent>]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void UpdateMovers(
		[Data] in Rect2 visRec,
		[Data] in float delta,
		[Data] in bool drawNavPaths,
		[Data] in ConcurrentQueue<Vector2[]> navLines,
		ref PositionComponent pos,
		ref VelocityComponent velocity,
		ref MoveSpeedComponent moveSpeed
	)
	{
		var isNear = visRec.HasPoint(pos.Position);
		if (isNear)
			MoveNavMover(delta, ref pos, ref moveSpeed, ref velocity, drawNavPaths, navLines);
		else
			MoveStraightMover(_playerPosition, delta, ref pos, ref velocity, ref moveSpeed);
	}

	private static void MoveStraightMover(
		in Vector2 moveToTarget,
		in float delta,
		ref PositionComponent pos,
		ref VelocityComponent velocity,
		ref MoveSpeedComponent moveSpeed
	)
	{
		MoveEnemy(
			ref pos.Position,
			ref velocity.Velocity,
			moveToTarget,
			delta,
			moveSpeed.MoveSpeed,
			moveSpeed.TurnSpeed
		);
	}

	private static void MoveEnemy(
		ref Vector2 pos,
		ref Vector2 velocity,
		Vector2 target,
		float delta,
		float moveSpeed,
		float turnSpeed
	)
	{
		velocity = velocity.Lerp(pos.DirectionTo(target) * moveSpeed, turnSpeed * delta);
		pos += velocity * delta;
	}

	private static void MoveNavMover(
		float delta,
		ref PositionComponent pos,
		ref MoveSpeedComponent moveSpeed,
		ref VelocityComponent velocity,
		bool drawNavPaths = false,
		ConcurrentQueue<Vector2[]>? navLines = null
	)
	{
		var paths = NavMap.Instance.GetNavLine(pos.Position);
		if (paths.Length < 2)
			return;

		var targetPos = paths[1];
		foreach (var p in paths[2..])
		{
			if (pos.Position.DistanceTo(p) > 10)
			{
				targetPos = p;
				break;
			}
		}

		velocity.Velocity = velocity.Velocity.Lerp(
			pos.Position.DirectionTo(targetPos) * moveSpeed.MoveSpeed,
			moveSpeed.TurnSpeed * delta
		);
		pos.Position += velocity.Velocity * delta;

#if DEBUG
		if (drawNavPaths)
			navLines.Enqueue(paths.ToArray());
#endif
	}
}
