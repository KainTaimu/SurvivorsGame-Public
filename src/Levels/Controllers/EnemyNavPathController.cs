using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Arch.System;
using Arch.System.SourceGenerator;
using Game.Core.ECS;

namespace Game.Levels.Controllers;

public partial class EnemyNavPathController : Node2D
{
	[Export]
	public bool DrawNavPaths;

	[Export(PropertyHint.Range, "0,1,0.05")]
	private float CorneringSmoothingThreshold
	{
		get;
		set
		{
			field = value;
			_corneringSmoothingThresholdValue = value;
		}
	} = 0.8f;

	[Export(PropertyHint.Range, "5,200,5")]
	private float MinCornerDistance
	{
		get;
		set
		{
			field = value;
			_minCornerDistanceValue = value;
		}
	} = 20;

	[Export(PropertyHint.Range, "5,400,5")]
	private float MaxCornerDistance
	{
		get;
		set
		{
			field = value;
			_maxCornerDistanceValue = value;
		}
	} = 120f;

	[Export(PropertyHint.Range, "0,100,5")]
	private float MinPathPointSeparation
	{
		get;
		set
		{
			field = value;
			_minPathPointSeparationValue = value;
		}
	} = 30f;

	private readonly ConcurrentQueue<(Vector2[] points, Color color)> _lines = [];

	private static Vector2 _playerPosition;
	private static float _minCornerDistanceValue;
	private static float _maxCornerDistanceValue;
	private static float _corneringSmoothingThresholdValue;
	private static float _minPathPointSeparationValue;

	public double ProcessTime { get; private set; }

	public override void _Ready()
	{
		_minCornerDistanceValue = MinCornerDistance;
		_maxCornerDistanceValue = MaxCornerDistance;
		_corneringSmoothingThresholdValue = CorneringSmoothingThreshold;
		_minPathPointSeparationValue = MinPathPointSeparation;
	}

	public override void _PhysicsProcess(double delta)
	{
		var player = GameWorld.Instance.MainPlayer;
		_playerPosition = player.GlobalPosition;

		var start = Time.GetTicksMsec();
		UpdateMoversQuery(GameWorld.World, NavMap.Instance.GridVisibilityRect, (float)delta, DrawNavPaths, _lines);
		ProcessTime = Time.GetTicksMsec() - start;

#if DEBUG
		if (DrawNavPaths && Engine.GetProcessFrames() % 1 == 0)
		{
			QueueRedraw();
		}
#endif
	}

	public override void _Draw()
	{
		while (_lines.TryDequeue(out var line))
			DrawPolyline(line.points, line.color, 2, true);
		_lines.Clear();
	}

	[Query]
	[All<PositionComponent, VelocityComponent, MoveSpeedComponent>]
	[None<DyingMarkerComponent>]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void UpdateMovers(
		[Data] in Rect2 visRec,
		[Data] in float delta,
		[Data] in bool drawNavPaths,
		[Data] in ConcurrentQueue<(Vector2[] points, Color color)> navLines,
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
	}

	private static void MoveNavMover(
		float delta,
		ref PositionComponent pos,
		ref MoveSpeedComponent moveSpeed,
		ref VelocityComponent velocity,
		bool drawNavPaths = false,
		ConcurrentQueue<(Vector2[] points, Color color)>? navLines = null
	)
	{
		var paths = NavMap.Instance.GetNavLine(pos.Position);
		if (paths.Length == 2)
		{
			MoveStraightMover(_playerPosition, delta, ref pos, ref velocity, ref moveSpeed);
			return;
		}
		if (paths.Length < 3)
			return;

		// ALGORITHM CREDIT: https://www.gamedeveloper.com/programming/group-pathfinding-movement-in-rts-style-games
		var pA = paths[0];

		var minSepSq = _minPathPointSeparationValue * _minPathPointSeparationValue;
		var i = 1;
		while (i < paths.Length - 1 && pos.Position.DistanceSquaredTo(paths[i]) < minSepSq)
			i++;

		if (pos.Position.DistanceSquaredTo(paths[i]) < minSepSq)
		{
			MoveStraightMover(paths[^1], delta, ref pos, ref velocity, ref moveSpeed);
			return;
		}

		var pB = paths[i];
		var pC = i + 1 < paths.Length ? paths[i + 1] : paths[^1];

		var vC = pA - pB;
		var vHalfC = vC * 0.5f;

		var vCorrection = (pA - vHalfC).Clamp(_minCornerDistanceValue, _maxCornerDistanceValue);

		// 0 is perpendicular, 1 is parallel
		var vCosine = vCorrection.Dot(vHalfC) / (vCorrection.Length() * vHalfC.Length());
		if (vCosine > _corneringSmoothingThresholdValue)
			vCorrection *= -1;

		var vFinal = pB + vCorrection;

		velocity.Velocity = velocity.Velocity.Lerp(
			pos.Position.DirectionTo(vFinal) * moveSpeed.MoveSpeed,
			moveSpeed.TurnSpeed * delta
		);

		if (drawNavPaths)
		{
			// navLines?.Enqueue((paths.ToArray(), Colors.White));
			navLines?.Enqueue(([pB, vFinal], Colors.Red));
			navLines?.Enqueue(([pA, vFinal, pC], Colors.Green));
		}
	}
}
