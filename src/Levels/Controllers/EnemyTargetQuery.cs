/*
 * Commented methods are written by AI slop machine
 */

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using Game.Core.ECS;
using Game.Models;

namespace Game.Levels.Controllers;

public partial class EnemyTargetQuery : Node
{
	// BREAKING: Changing this value breaks Projectile radius of weapons
	private const int GRID_SIZE = 16;

	public UniformGridWorld<Entity> Grid => _grid;

	private UniformGridWorld<Entity> _grid = null!;

	private readonly ConcurrentDictionary<Entity, Vector2> _addBuffer = [];

	public static EnemyTargetQuery Instance { get; private set; } = null!;

	public override void _Ready()
	{
		Instance = this;
		var viewport = GetViewport();
		if (viewport is null)
		{
			Logger.LogError("missing viewport.");
			return;
		}

		var windowSize = viewport.GetVisibleRect().Size * 1.3f;
		_grid = new UniformGridWorld<Entity>(GRID_SIZE, windowSize);
	}

	public override void _Process(double delta)
	{
		var player = GameWorld.Instance.MainPlayer;
		var playerPos = player.GlobalPosition;

		_grid.ClearAll();
		_grid.Recenter(playerPos);
		_addBuffer.Clear();
		AddObjectsToGridQuery(GameWorld.World, _grid, _addBuffer);

		foreach (var (entity, pos) in _addBuffer)
			_grid.AddWorld(pos, entity);
	}

	// causes a build warning if you change this method's signature
	// ReSharper disable once MemberCanBeMadeStatic.Local
#pragma warning disable CA1822
	[Query(Parallel = true)]
	[All<PositionComponent, CircleHitboxComponent>]
	[None<DyingMarkerComponent>]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void AddObjectsToGrid(
#pragma warning restore CA1822
		[Data] in UniformGridWorld<Entity> grid,
		[Data] in ConcurrentDictionary<Entity, Vector2> addBuffer,
		in Entity entity,
		ref PositionComponent pos
	)
	{
		if (!grid.ContainsWorld(pos.Position))
			return;

		addBuffer[entity] = pos.Position;
	}

	private bool CircleHitTest(Vector2 projPos, float projRadius, Entity entity)
	{
		if (!GameWorld.World.TryGet<PositionComponent>(entity, out var pos))
			return false;
		if (!GameWorld.World.TryGet<CircleHitboxComponent>(entity, out var hitbox))
			return false;

		var radiusSum = projRadius + hitbox.Radius;
		return pos.Position.DistanceSquaredTo(projPos) <= radiusSum * radiusSum;
	}

	public bool TryGetTargetsInArea(Vector2 areaCenter, float areaRadius, out Entity[] targetIds)
	{
		// credit: https://www.redblobgames.com/grids/circle-drawing/
		var targets = new List<Entity>();

		var top = Math.Ceiling(areaCenter.Y - areaRadius);
		var bottom = Math.Floor(areaCenter.Y + areaRadius);
		for (var y = top; y <= bottom; y++)
		{
			var dy = y - areaCenter.Y;
			var dx = Math.Sqrt(areaRadius * areaRadius - dy * dy);
			var left = Math.Ceiling(areaCenter.X - dx);
			var right = Math.Floor(areaCenter.X + dx);
			for (var x = left; x <= right; x++)
			{
				var cell = _grid.WorldToCell(new Vector2((float)x, (float)y));
				if (!_grid.IsValidCell(cell.X, cell.Y))
					continue;

				foreach (var entity in _grid.GetEnumerator(cell.X, cell.Y))
				{
					if (targets.Contains(entity))
						continue;
					if (CircleHitTest(areaCenter, areaRadius, entity))
						targets.Add(entity);
				}
			}
		}

		targetIds = [.. targets];
		return targets.Count > 0;
	}

	public IEnumerable<Entity> GetTargetsInScreen()
	{
		for (var x = 0; x < _grid.Dimensions.X; x++)
		{
			for (var y = 0; y < _grid.Dimensions.Y; y++)
			{
				foreach (var entity in _grid.GetEnumerator(x, y))
					yield return entity;
			}
		}
	}

	/// <summary>
	/// Samples all grid cells along a line forming an angle, with thickness.
	/// Uses Bresenham line algorithm to traverse grid cells.
	/// </summary>
	/// <param name="from">Ray origin in world coordinates.</param>
	/// <param name="angle">Ray angle in radians.</param>
	/// <param name="width">Ray thickness (radius perpendicular to direction).</param>
	/// <param name="entities">Entities caught in ray. Sorted by distance</param>
	/// <param name="hitLimit">How many hits before giving up</param>
	/// <returns>
	/// <c>true</c> when at least one target lies within raycast corridor.
	/// </returns>
	public bool GetTargetsRayCast(Vector2 from, float angle, float width, out Entity[] entities, int hitLimit = -1)
	{
		var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
		var rayLength = _grid.WindowSize.Length();
		var rayEnd = from + direction * rayLength;

		var cellsOnRay = new HashSet<Vector2I>();
		GetCellsAlongLine(from, rayEnd, cellsOnRay);

		var cellsOnRayList = cellsOnRay.ToList();
		cellsOnRayList.Sort(
			(a, b) =>
				(new Vector2(a.X, a.Y) * GRID_SIZE + _grid.TopLeft - from)
					.LengthSquared()
					.CompareTo((new Vector2(b.X, b.Y) * GRID_SIZE + _grid.TopLeft - from).LengthSquared())
		);

		var allCells = new List<Vector2I>();
		foreach (var cellIdx in cellsOnRayList)
		{
			var surrounding = new List<Vector2I>();
			for (var dx = -3; dx <= 3; dx++)
			for (var dy = -3; dy <= 3; dy++)
			{
				var checkCell = new Vector2I(cellIdx.X + dx, cellIdx.Y + dy);
				if (_grid.IsValidCell(checkCell.X, checkCell.Y))
					surrounding.Add(checkCell);
			}

			surrounding.Sort(
				(a, b) =>
				{
					var cellWorldA = new Vector2(a.X * GRID_SIZE, a.Y * GRID_SIZE) + _grid.TopLeft;
					var cellWorldB = new Vector2(b.X * GRID_SIZE, b.Y * GRID_SIZE) + _grid.TopLeft;
					return DistanceSquaredPointToRay(cellWorldA, from, direction, rayLength)
						.CompareTo(DistanceSquaredPointToRay(cellWorldB, from, direction, rayLength));
				}
			);

			allCells.AddRange(surrounding);
		}

		var targets = new List<Entity>();
		var seenIds = new HashSet<Entity>();

		var hitCount = 0;

		foreach (var cellIndex in allCells)
		{
			foreach (var entity in _grid.GetEnumerator(cellIndex.X, cellIndex.Y))
			{
				if (!GameWorld.World.IsAlive(entity))
					continue;
				if (!seenIds.Add(entity))
					continue;

				if (!GameWorld.World.TryGet<PositionComponent>(entity, out var pos))
					continue;
				if (!GameWorld.World.TryGet<CircleHitboxComponent>(entity, out var hitbox))
					continue;

				var distSq = DistanceSquaredPointToRay(pos.Position, from, direction, rayLength);
				var radiusSum = width + hitbox.Radius;
				if (distSq <= radiusSum * radiusSum)
				{
					targets.Add(entity);
					hitCount++;
					if (hitCount != -1 && hitCount >= hitLimit)
						goto Exit;
				}
			}
		}

		Exit:
		entities = [.. targets];
		return targets.Count > 0;
	}

	private void GetCellsAlongLine(Vector2 start, Vector2 end, HashSet<Vector2I> cells)
	{
		var startCell = _grid.WorldToCell(start);
		if (!_grid.IsValidCell(startCell.X, startCell.Y))
			return;

		var direction = (end - start).Normalized();
		var maxDist = (end - start).Length();
		var current = startCell;
		var dx = Mathf.Abs(Mathf.RoundToInt(direction.X * maxDist));
		var dy = Mathf.Abs(Mathf.RoundToInt(direction.Y * maxDist));
		var sx = direction.X > 0 ? 1 : -1;
		var sy = direction.Y > 0 ? 1 : -1;
		var err = dx - dy;

		cells.Add(current);

		while (true)
		{
			var e2 = 2 * err;
			if (e2 > -dy)
			{
				err -= dy;
				current.X += sx;
			}

			if (e2 < dx)
			{
				err += dx;
				current.Y += sy;
			}

			if (!_grid.IsValidCell(current.X, current.Y))
				break;

			cells.Add(current);
		}
	}

	private static float DistanceSquaredPointToRay(
		Vector2 point,
		Vector2 rayStart,
		Vector2 rayDirection,
		float rayLength
	)
	{
		var toPoint = point - rayStart;
		var t = toPoint.Dot(rayDirection);

		if (t < 0f)
			return toPoint.LengthSquared();
		if (t > rayLength)
			return point.DistanceSquaredTo(rayStart + rayDirection * rayLength);

		return point.DistanceSquaredTo(rayStart + rayDirection * t);
	}
}
