// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using Game.Core.ECS;
using Game.Models;
using Game.UI;
using Godot.Collections;
using Array = System.Array;

namespace Game.Levels.Controllers;

[GlobalClass]
public partial class EnemyCollisionSolverCpu : AbstractEnemyCollisionSolver, IMultipleFrameTimeTrackable
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

	private CenteredMovingUniformGrid<(Vector2 pos, Entity entity, float radius)> _grid = null!;

	private (Vector2 pos, Entity entity, float radius)[] _entries = [];

	private int[] _stamps = [];
	private int _writeFrame;

	private Vector2 _playerPosition;

	private Action<int>? _solveColumnEven;
	private Action<int>? _solveColumnOdd;

	[Export]
	public Array<FrameTime> FrameTimes { get; private set; } = [];

	public override void _Ready()
	{
		var viewport = GetViewport();
		if (viewport is null)
		{
			CustomLogger.LogError("missing viewport.");
			return;
		}

		var windowSize = viewport.GetVisibleRect().Size * _solverRangeFactor;

		// _grid uses a square due to enemy "spill" when large amounts of
		// enemies previously not affected by the collision solver are
		// suddenly affected by it, causing a large amount of enemies
		// to be shoved towards player. The smaller height of 16:9 display
		// makes it harder for player to avoid the spilled enemies.
		_grid = new CenteredMovingUniformGrid<(Vector2, Entity, float)>(
			_gridSize,
			new Vector2(windowSize.X, windowSize.X)
		);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!Enabled)
			return;

		var player = GameWorld.Instance.MainPlayer;
		_grid.Recenter(player.GlobalPosition);
		_playerPosition = player.GlobalPosition;

		_writeFrame++;
		if (_writeFrame == int.MaxValue)
		{
			_writeFrame = 1;
			Array.Clear(_stamps);
		}

		EnsureBufferCapacity(GameWorld.World.Capacity);

		using (FrameTime.Record())
		{
			AddObjectsToGridQuery(GameWorld.World, _grid, _entries, _stamps, _writeFrame);
			using (FrameTimes[0].Record())
				for (var i = 0; i < SubSteps; i++)
				{
					_grid.ClearGrid();
					AddObjectsToGridFromBuffer();
					SolveCollisions();
				}

			using (FrameTimes[1].Record())
				ApplyCollisionsQuery(GameWorld.World, _entries, _stamps, _writeFrame, NavMap!);
		}
	}

	private void EnsureBufferCapacity(int capacity)
	{
		if (_entries.Length >= capacity)
			return;
		CustomLogger.LogDebug("resize" + capacity);

		var newSize = Math.Max(capacity, _entries.Length * 2);
		Array.Resize(ref _entries, newSize);
		Array.Resize(ref _stamps, newSize);
	}

	[Query(Parallel = true)]
	[All<PositionComponent, CollisionLodComponent, CircleHitboxComponent>]
	[None<DyingMarkerComponent>]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void AddObjectsToGrid(
		[Data] in CenteredMovingUniformGrid<(Vector2 pos, Entity entity, float radius)> grid,
		[Data] in (Vector2 pos, Entity entity, float radius)[] entries,
		[Data] in int[] stamps,
		[Data] in int writeFrame,
		Entity entity,
		ref PositionComponent pos,
		ref CircleHitboxComponent circle
	)
	{
		if (!grid.ContainsWorld(pos.Position))
			return;

		var id = entity.Id;
		if ((uint)id >= (uint)entries.Length)
			return;

		entries[id] = (pos.Position, entity, circle.Radius);
		stamps[id] = writeFrame;
	}

	private void AddObjectsToGridFromBuffer()
	{
		for (var i = 0; i < _entries.Length; i++)
		{
			if (_stamps[i] != _writeFrame)
				continue;

			var entry = _entries[i];
			_grid.TryAddWorld(entry.pos, entry);
		}
	}

	[Query(Parallel = true)]
	[All<PositionComponent, CircleHitboxComponent>]
	[None<DyingMarkerComponent>]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void ApplyCollisions(
		[Data] in (Vector2 pos, Entity entity, float radius)[] entries,
		[Data] in int[] stamps,
		[Data] in int writeFrame,
		[Data] in NavMap navMap,
		Entity entity,
		ref PositionComponent pos
	)
	{
		var id = entity.Id;
		if ((uint)id >= (uint)stamps.Length || stamps[id] != writeFrame)
			return;

		var newPos = entries[id].pos;
		// Arch does not support nullable operator in parameters
		// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
		if (
			navMap is not null
			&& navMap.GridVisibilityRect.HasPoint(pos.Position)
			&& (navMap.WallProximity is null || navMap.WallProximity.NeedsClamp(newPos))
		)
			pos.Position = NavigationServer2D.MapGetClosestPoint(NavMap.Map, newPos);
		else
			pos.Position = newPos;
	}

	// Columns are solved in two phases (even, then odd). A column only
	// interacts with itself and its eastern neighbor, so columns two apart
	// never touch the same entities. This keeps the dense buffer writes in
	// SolveCollisionInPlace race-free without any locking.
	private void SolveCollisions()
	{
		SolveColumnPhaseEven();
		SolveColumnPhaseOdd();
	}

	private void SolveColumnPhaseEven()
	{
		var columns = (_grid.Dimensions.X + 1) / 2;
		_solveColumnEven ??= BodyEven;
		Parallel.For(0, columns, _solveColumnEven);
	}

	private void SolveColumnPhaseOdd()
	{
		var columns = (_grid.Dimensions.X) / 2;
		_solveColumnOdd ??= BodyOdd;
		Parallel.For(0, columns, _solveColumnOdd);
	}

	private void BodyEven(int i)
	{
		var x = i * 2;
		for (var y = 0; y < _grid.Dimensions.Y; y++)
		{
			var cellIdx = _grid.GetCellIndex(x, y);
			if (cellIdx < 0)
				continue;

			ref var cell = ref _grid.Cells[cellIdx];
			if (cell.Count <= 1 || cell.Count > 10)
				continue;

			SolveCellInternalCollisions(ref cell);

			SolveCellPairCollisions(ref cell, _grid.GetCellIndex(x + 1, y)); // E
			SolveCellPairCollisions(ref cell, _grid.GetCellIndex(x, y + 1)); // S
			SolveCellPairCollisions(ref cell, _grid.GetCellIndex(x + 1, y + 1)); // SE
			SolveCellPairCollisions(ref cell, _grid.GetCellIndex(x + 1, y - 1)); // NE
		}
	}

	private void BodyOdd(int i)
	{
		var x = i * 2 + 1;
		for (var y = 0; y < _grid.Dimensions.Y; y++)
		{
			var cellIdx = _grid.GetCellIndex(x, y);
			if (cellIdx < 0)
				continue;

			ref var cell = ref _grid.Cells[cellIdx];
			if (cell.Count <= 1 || cell.Count > 10)
				continue;

			SolveCellInternalCollisions(ref cell);

			SolveCellPairCollisions(ref cell, _grid.GetCellIndex(x + 1, y)); // E
			SolveCellPairCollisions(ref cell, _grid.GetCellIndex(x, y + 1)); // S
			SolveCellPairCollisions(ref cell, _grid.GetCellIndex(x + 1, y + 1)); // SE
			SolveCellPairCollisions(ref cell, _grid.GetCellIndex(x + 1, y - 1)); // NE
		}
	}

	private void SolveCellInternalCollisions(ref UniformGridCell<(Vector2 pos, Entity entity, float radius)> cell)
	{
		for (var i = 0; i < cell.Count; i++)
		{
			for (var j = i + 1; j < cell.Count; j++)
				SolveCollisionInPlace(ref cell, i, ref cell, j);
		}
	}

	private void SolveCellPairCollisions(
		ref UniformGridCell<(Vector2 pos, Entity entity, float radius)> cellA,
		int cellBIndex
	)
	{
		if (cellBIndex < 0)
			return;

		ref var cellB = ref _grid.Cells[cellBIndex];
		if (cellB.Count == 0)
			return;

		for (var i = 0; i < cellA.Count; i++)
		{
			for (var j = 0; j < cellB.Count; j++)
				SolveCollisionInPlace(ref cellA, i, ref cellB, j);
		}
	}

	private void SolveCollisionInPlace(
		ref UniformGridCell<(Vector2 pos, Entity entity, float radius)> cellA,
		int indexA,
		ref UniformGridCell<(Vector2 pos, Entity entity, float radius)> cellB,
		int indexB
	)
	{
		var (posA, entityA, radiusA) = cellA.Items[indexA];
		var (posB, entityB, radiusB) = cellB.Items[indexB];

		if (entityA == entityB)
			return;

		var largest = Math.Max(radiusA, radiusB) * 3;
		if (posA.DistanceSquaredTo(posB) >= largest * largest)
			return;

		var direction = posB.DirectionTo(posA);
		if (direction == Vector2.Zero)
			direction = Vector2.Right;

		var push = _distBeforeShove * 0.5f * _pushAmount;

		if (cellA.Count >= _cramLimitBeforeExtraPush || cellB.Count >= _cramLimitBeforeExtraPush)
		{
			var extraPush = Mathf.Log((cellA.Count + cellB.Count) / 1.5f) * _cramExtraPushFactor;
			extraPush = Math.Abs(extraPush);
			push *= Math.Abs(extraPush);
		}

		posA += direction * push;
		posB -= direction * push;

		cellA.Items[indexA] = (posA, entityA, radiusA);
		cellB.Items[indexB] = (posB, entityB, radiusB);

		_entries[entityA.Id].pos = posA;
		_entries[entityB.Id].pos = posB;
	}
}
