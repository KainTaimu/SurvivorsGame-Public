using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using Game.Core.ECS;
using Game.Models;

namespace Game.Levels.Controllers;

public partial class EnemyCollisionSolverCpu : AbstractEnemyCollisionSolver
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

	private UniformGridWorld<(Vector2 pos, Entity entity, float radius)> _grid = null!;

	// Dense write buffers indexed by Entity.Id. Entries are valid for the
	// current frame when _stamps[id] == _writeFrame, avoiding a full clear.
	private (Vector2 pos, Entity entity, float radius)[] _entries = [];

	private int[] _stamps = [];
	private int _writeFrame;

	private Vector2 _playerPosition;

	public override void _Ready()
	{
		var viewport = GetViewport();
		if (viewport is null)
		{
			Logger.LogError("missing viewport.");
			return;
		}

		var windowSize = viewport.GetVisibleRect().Size * _solverRangeFactor;

		// _grid uses a square due to enemy "spill" when large amounts of
		// enemies previously not affected by the collision solver are
		// suddenly affected by it, causing a large amount of enemies
		// to be shoved towards player. The smaller height of 16:9 display
		// makes it harder for player to avoid the spilled enemies.
		_grid = new UniformGridWorld<(Vector2, Entity, float)>(
			_gridSize,
			new Vector2(windowSize.X, windowSize.X),
			initialCapacity: 128
		);

		Logger.LogDebug("in", _grid.Dimensions, _grid.CellSize);
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
			for (var i = 0; i < SubSteps; i++)
			{
				_grid.ClearAll();
				AddObjectsToGridFromBuffer();
				SolveCollisions();
			}

			ApplyCollisionsQuery(GameWorld.World, _entries, _stamps, _writeFrame, _navMap!);
		}
	}

	private void EnsureBufferCapacity(int capacity)
	{
		if (_entries.Length >= capacity)
			return;

		var newSize = Math.Max(capacity, _entries.Length * 2);
		Array.Resize(ref _entries, newSize);
		Array.Resize(ref _stamps, newSize);
	}

	[Query(Parallel = true)]
	[All<PositionComponent, CollisionLodComponent, CircleHitboxComponent>]
	[None<DyingMarkerComponent>]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void AddObjectsToGrid(
		[Data] in UniformGridWorld<(Vector2 pos, Entity entity, float radius)> grid,
		[Data] in (Vector2 pos, Entity entity, float radius)[] entries,
		[Data] in int[] stamps,
		[Data] in int writeFrame,
		in Entity entity,
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

			_grid.AddWorld(entry.pos, entry);
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
		in Entity entity,
		ref PositionComponent pos
	)
	{
		var id = entity.Id;
		if ((uint)id >= (uint)stamps.Length || stamps[id] != writeFrame)
			return;

		var newPos = entries[id].pos;
		// Arch does not support nullable operator in parameters
		// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
		if (navMap is not null && navMap.GridVisibilityRect.HasPoint(pos.Position))
		{
			pos.Position = NavigationServer2D.MapGetClosestPoint(NavMap.Map, newPos);
		}
		else
			pos.Position = newPos;
	}

	// Columns are solved in two phases (even, then odd). A column only
	// interacts with itself and its eastern neighbor, so columns two apart
	// never touch the same entities. This keeps the dense buffer writes in
	// SolveCollisionInPlace race-free without any locking.
	private void SolveCollisions()
	{
		SolveColumnPhase(0);
		SolveColumnPhase(1);
	}

	private void SolveColumnPhase(int phase)
	{
		var columns = (_grid.Dimensions.X + 1 - phase) / 2;
		var id = WorkerThreadPool.AddGroupTask(
			Callable.From<int>(i =>
			{
				var x = i * 2 + phase;
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
			columns
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
		ref (Vector2 pos, Entity entity, float radius) a,
		ref (Vector2 pos, Entity entity, float radius) b,
		int countA,
		int countB
	)
	{
		if (a.entity == b.entity)
			return;

		var largest = Math.Max(a.radius, b.radius) * 3;
		if (a.pos.DistanceSquaredTo(b.pos) >= largest * largest)
			return;

		// if (a.pos.DistanceSquaredTo(b.pos) >= _distBeforeShove * _distBeforeShove)
		// 	return;

		var direction = b.pos.DirectionTo(a.pos);
		if (direction == Vector2.Zero)
			direction = Vector2.Right;

		var push = _distBeforeShove * 0.5f * _pushAmount;

		if (countA >= _cramLimitBeforeExtraPush || countB >= _cramLimitBeforeExtraPush)
		{
			var extraPush = Mathf.Log((countA + countB) / 1.5f) * _cramExtraPushFactor;
			extraPush = Math.Abs(extraPush);
			push *= Math.Abs(extraPush);
		}

		a.pos += direction * push;
		b.pos -= direction * push;

		_entries[a.entity.Id].pos = a.pos;
		_entries[b.entity.Id].pos = b.pos;
	}
}
