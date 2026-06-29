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
	private CenteredMovingUniformGrid<(Vector2, Entity)> _grid = null!;

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
		_grid = new CenteredMovingUniformGrid<(Vector2, Entity)>(_gridSize, new Vector2(windowSize.X, windowSize.X));

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
			_grid.ClearGrid();
			AddObjectsToGridFromBuffer();
			SolveCollisions();
			if (i > 1 && Time.GetTicksMsec() - start > 9)
				break;
		}

		ApplyCollisionsQuery(GameWorld.World, _writeBuffer);
		ProcessTime = Time.GetTicksMsec() - start;
	}

	[Query(Parallel = true)]
	[All<PositionComponent, CollidableComponent, CircleHitboxComponent>]
	[None<DyingMarkerComponent>]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void AddObjectsToGrid(
		[Data] in CenteredMovingUniformGrid<(Vector2, Entity)> grid,
		[Data] in ConcurrentDictionary<Entity, Vector2> writeBuffer,
		[Data] in ConcurrentDictionary<Entity, float> entityCollisionRadius,
		in Entity entity,
		ref PositionComponent pos,
		ref CircleHitboxComponent circle
	)
	{
		if (!grid.ContainsWorld(pos.Position))
			return;

		var cell = grid.GetCellWorld(pos.Position);
		cell?.Add((pos.Position, entity));
		writeBuffer[entity] = pos.Position;
		entityCollisionRadius.TryAdd(entity, circle.Radius);
	}

	private void AddObjectsToGridFromBuffer()
	{
		foreach (var (id, pos) in _writeBuffer)
		{
			if (!_grid.ContainsWorld(pos))
				continue;

			var cell = _grid.GetCellWorld(pos);
			cell?.Add((pos, id));
		}
	}

	[Query(Parallel = true)]
	[All<PositionComponent, CircleHitboxComponent>]
	[None<DyingMarkerComponent>]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void ApplyCollisions(
		[Data] in ConcurrentDictionary<Entity, Vector2> writeBuffer,
		in Entity entity,
		ref PositionComponent pos
	)
	{
		if (writeBuffer.TryGetValue(entity, out var newPos))
			pos.Position = newPos;
	}

	private void SolveCollisions()
	{
		var id = WorkerThreadPool.AddGroupTask(
			Callable.From<int>(x =>
			{
				for (var y = 0; y < _grid.Dimensions.Y; y++)
				{
					var cell = _grid.GetCell(x, y);
					if (cell is null || cell.Count <= 1 || cell.Count > 10)
						continue;

					SolveCellInternalCollisions(cell);

					SolveCellPairCollisions(cell, _grid.GetCell(x + 1, y)); // E
					SolveCellPairCollisions(cell, _grid.GetCell(x, y + 1)); // S
					SolveCellPairCollisions(cell, _grid.GetCell(x + 1, y + 1)); // SE
					SolveCellPairCollisions(cell, _grid.GetCell(x + 1, y - 1)); // NE
				}
			}),
			_grid.Dimensions.X
		);
		WorkerThreadPool.WaitForGroupTaskCompletion(id);
	}

	private void SolveCellInternalCollisions(UniformGridCell<(Vector2 pos, Entity entity)> cell)
	{
		for (var i = 0; i < cell.Count; i++)
		{
			for (var j = i + 1; j < cell.Count; j++)
				SolveCollisionInPlace(cell, i, cell, j);
		}
	}

	private void SolveCellPairCollisions(
		UniformGridCell<(Vector2 pos, Entity entity)> cellA,
		UniformGridCell<(Vector2 pos, Entity entity)>? cellB
	)
	{
		if (cellB is null || cellB.Count == 0)
			return;

		for (var i = 0; i < cellA.Count; i++)
		{
			for (var j = 0; j < cellB.Count; j++)
				SolveCollisionInPlace(cellA, i, cellB, j);
		}
	}

	private void SolveCollisionInPlace(
		UniformGridCell<(Vector2 pos, Entity entity)> cellA,
		int indexA,
		UniformGridCell<(Vector2 pos, Entity entity)> cellB,
		int indexB
	)
	{
		var (posA, idA) = cellA.Array[indexA];
		var (posB, idB) = cellB.Array[indexB];

		if (idA == idB)
			return;

		if (
			!_entityCollisionRadius.TryGetValue(idA, out var radiusA)
			|| !_entityCollisionRadius.TryGetValue(idB, out var radiusB)
		)
			return;

		var largest = Math.Max(radiusA, radiusB) * 3;
		if (posA.DistanceSquaredTo(posB) >= largest * largest)
			return;

		// if (posA.DistanceSquaredTo(posB) >= _distBeforeShove * _distBeforeShove)
		// 	return;

		var direction = posB.DirectionTo(posA);
		if (direction == Vector2.Zero)
			direction = Vector2.Right;

		// NOTE: Delta is accounted for in the Timer that calls Process.
		var push = _distBeforeShove * 0.5f * _pushAmount;

		if (cellA.Count >= _cramLimitBeforeExtraPush || cellB.Count >= _cramLimitBeforeExtraPush)
		{
			var extraPush = Mathf.Log((cellA.Count + cellB.Count) / 1.5f) * _cramExtraPushFactor;
			extraPush = Math.Abs(extraPush);
			push *= Math.Abs(extraPush);
		}

		posA += direction * push;
		posB -= direction * push;

		cellA.Array[indexA] = (posA, idA);
		cellB.Array[indexB] = (posB, idB);

		_writeBuffer[idA] = posA;
		_writeBuffer[idB] = posB;
	}
}
