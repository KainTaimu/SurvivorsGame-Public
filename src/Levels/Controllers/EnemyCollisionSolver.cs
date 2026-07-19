using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using Game.Core.ECS;
using Game.Models;

namespace Game.Levels.Controllers;

public partial class EnemyCollisionSolver : Node
{
	[Export]
	private NavMap? _navMap;

	[ExportCategory("Configuration")]
	[Export]
	private byte _gridSize = 64;

	[Export]
	private float _solverRangeFactor = 3f;

	[Export]
	private int _distBeforeShove = 50;

	[Export(PropertyHint.Range, "0,5,0.1")]
	private float _pushAmount = 0.4f;

	[Export]
	private int _cramLimitBeforeExtraPush = 6;

	[Export]
	private float _cramExtraPushFactor = 6f;

	[ExportCategory("Toggles")]
	[Export]
	public bool Enabled = true;

	[Export]
	public byte SubSteps = 6;

	[ExportCategory("Components")]
	private UniformGridWorld<(Vector2, Entity)> _grid = null!;

	private readonly ConcurrentDictionary<Entity, float> _entityCollisionRadius = [];

	private readonly ConcurrentDictionary<Entity, Vector2> _writeBuffer = [];

	public double ProcessTime { get; private set; }

	private Vector2 _playerPosition;

	public override void _Ready()
	{
		var viewport = GetViewport();
		if (viewport is null)
		{
			Logger.LogError("EnemyCollisionSolver: missing viewport.");
			return;
		}

		var windowSize = viewport.GetVisibleRect().Size * _solverRangeFactor;

		// _grid uses a square due to enemy "spill" when large amounts of
		// enemies previously not affected by the collision solver are
		// suddenly affected by it, causing a large amount of enemies
		// to be shoved towards player. The smaller height of 16:9 display
		// makes it harder for player to avoid the spilled enemies.
		_grid = new UniformGridWorld<(Vector2, Entity)>(_gridSize, new Vector2(windowSize.X, windowSize.X));

		Logger.LogDebug("in", _grid.Dimensions, _grid.CellSize);
	}

	public override void _Process(double delta)
	{
		if (!Enabled)
			return;

		var player = GameWorld.Instance.MainPlayer;
		_grid.Recenter(player.GlobalPosition);
		_playerPosition = player.GlobalPosition;

		_writeBuffer.Clear();
		_entityCollisionRadius.Clear();

		var start = Time.GetTicksMsec();
		AddObjectsToGridQuery(GameWorld.World, _grid, _writeBuffer, _entityCollisionRadius);
		for (var i = 0; i < SubSteps; i++)
		{
			_grid.ClearAll();
			AddObjectsToGridFromBuffer();
			SolveCollisions();
			if (i > 1 && Time.GetTicksMsec() - start > 9)
				break;
		}

		ApplyCollisionsQuery(GameWorld.World, _writeBuffer, _navMap!);
		ProcessTime = Time.GetTicksMsec() - start;
	}

	[Query(Parallel = true)]
	[All<PositionComponent, CollisionLodComponent, CircleHitboxComponent>]
	[None<DyingMarkerComponent>]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void AddObjectsToGrid(
		[Data] in UniformGridWorld<(Vector2, Entity)> grid,
		[Data] in ConcurrentDictionary<Entity, Vector2> writeBuffer,
		[Data] in ConcurrentDictionary<Entity, float> entityCollisionRadius,
		in Entity entity,
		ref PositionComponent pos,
		ref CircleHitboxComponent circle
	)
	{
		if (!grid.ContainsWorld(pos.Position))
			return;

		writeBuffer[entity] = pos.Position;
		entityCollisionRadius.TryAdd(entity, circle.Radius);
	}

	private void AddObjectsToGridFromBuffer()
	{
		foreach (var (id, pos) in _writeBuffer)
		{
			if (!_grid.ContainsWorld(pos))
				continue;

			_grid.AddWorld(pos, (pos, id));
		}
	}

	private static readonly ConcurrentDictionary<Vector2, Vector2> _nearest = [];

	[Query(Parallel = true)]
	[All<PositionComponent, CircleHitboxComponent>]
	[None<DyingMarkerComponent>]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void ApplyCollisions(
		[Data] in ConcurrentDictionary<Entity, Vector2> writeBuffer,
		[Data] in NavMap navMap,
		in Entity entity,
		ref PositionComponent pos
	)
	{
		if (!writeBuffer.TryGetValue(entity, out var newPos))
			return;
		// Arch does not support nullable operator in parameters
		// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
		if (navMap is not null && navMap.GridVisibilityRect.HasPoint(pos.Position))
		{
			pos.Position = NavigationServer2D.MapGetClosestPoint(NavMap.Map, newPos);
		}
		else
			pos.Position = newPos;
	}

	private void SolveCollisions()
	{
		var id = WorkerThreadPool.AddGroupTask(
			Callable.From<int>(x =>
			{
				for (var y = 0; y < _grid.Dimensions.Y; y++)
				{
					var count = _grid.GetCellCount(x, y);
					if (count <= 1 || count > 10)
						continue;

					SolveCellInternalCollisions(x, y, count);

					SolveCellPairCollisions(x, y, x + 1, y); // E
					SolveCellPairCollisions(x, y, x, y + 1); // S
					SolveCellPairCollisions(x, y, x + 1, y + 1); // SE
					SolveCellPairCollisions(x, y, x + 1, y - 1); // NE
				}
			}),
			_grid.Dimensions.X
		);
		WorkerThreadPool.WaitForGroupTaskCompletion(id);
	}

	private void SolveCellInternalCollisions(int x, int y, int count)
	{
		var outer = _grid.GetEnumerator(x, y);
		while (outer.MoveNext())
		{
			var rest = outer.CloneRest();
			while (rest.MoveNext())
				SolveCollisionInPlace(ref outer.CurrentRef, ref rest.CurrentRef, count, count);
		}
	}

	private void SolveCellPairCollisions(int ax, int ay, int bx, int by)
	{
		if (!_grid.IsValidCell(bx, by))
			return;

		var countA = _grid.GetCellCount(ax, ay);
		var countB = _grid.GetCellCount(bx, by);
		if (countB == 0)
			return;

		var enumA = _grid.GetEnumerator(ax, ay);
		while (enumA.MoveNext())
		{
			var enumB = _grid.GetEnumerator(bx, by);
			while (enumB.MoveNext())
				SolveCollisionInPlace(ref enumA.CurrentRef, ref enumB.CurrentRef, countA, countB);
		}
	}

	private void SolveCollisionInPlace(
		ref (Vector2 pos, Entity entity) a,
		ref (Vector2 pos, Entity entity) b,
		int countA,
		int countB
	)
	{
		if (a.entity == b.entity)
			return;

		if (
			!_entityCollisionRadius.TryGetValue(a.entity, out var radiusA)
			|| !_entityCollisionRadius.TryGetValue(b.entity, out var radiusB)
		)
			return;

		var largest = Math.Max(radiusA, radiusB) * 3;
		if (a.pos.DistanceSquaredTo(b.pos) >= largest * largest)
			return;

		// if (a.pos.DistanceSquaredTo(b.pos) >= _distBeforeShove * _distBeforeShove)
		// 	return;

		var direction = b.pos.DirectionTo(a.pos);
		if (direction == Vector2.Zero)
			direction = Vector2.Right;

		// NOTE: Delta is accounted for in the Timer that calls Process.
		var push = _distBeforeShove * 0.5f * _pushAmount;

		if (countA >= _cramLimitBeforeExtraPush || countB >= _cramLimitBeforeExtraPush)
		{
			var extraPush = Mathf.Log((countA + countB) / 1.5f) * _cramExtraPushFactor;
			extraPush = Math.Abs(extraPush);
			push *= Math.Abs(extraPush);
		}

		a.pos += direction * push;
		b.pos -= direction * push;

		_writeBuffer[a.entity] = a.pos;
		_writeBuffer[b.entity] = b.pos;
	}
}
