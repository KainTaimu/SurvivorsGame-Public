// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using Game.Core.ECS;
using Game.Core.Extensions;
using Game.Models;
using Game.Players.Controllers;
using Game.UI;
using Godot.Collections;

namespace Game.Levels.Controllers;

[GlobalClass]
public partial class EnemyTargetQuery : Node, IMultipleFrameTimeTrackable
{
	[Export]
	private float _rangeFactor = 1.5f;

	[Export]
	public FrameTime FrameTime { get; private set; } = null!;

	[Export]
	public Array<FrameTime> FrameTimes { get; private set; } = [];

	// BREAKING: Changing this value breaks Projectile radius of weapons
	private const int GRID_SIZE = 16;

	private CenteredMovingUniformGrid<Entity> _grid = null!;

	public static EnemyTargetQuery Instance { get; private set; } = null!;

	public override void _Ready()
	{
		Instance = this;
		var viewport = GetViewport();

		if (viewport.GetCamera2D() is SignalCamera2D cam)
			cam.OnCurrentZoomChanged += (_, _) => CreateGrid();
		CreateGrid();
	}

	private void CreateGrid()
	{
		var viewport = GetViewport();
		if (viewport is null)
		{
			CustomLogger.LogError("missing viewport.");
			return;
		}

		var zoom = viewport.GetCamera2D().Zoom.GetLargestComponent();

		var visRect = viewport.GetVisibleRect();
		var windowSize = visRect.Size * (1f / zoom) * _rangeFactor;
		_grid = new CenteredMovingUniformGrid<Entity>(GRID_SIZE, windowSize);
	}

	public override void _Process(double delta)
	{
		var player = GameWorld.Instance.MainPlayer;
		var playerPos = player.GlobalPosition;

		_grid.ClearGrid();
		_grid.Recenter(playerPos);
		AddObjectsToGridQuery(GameWorld.World, _grid);
	}

	[Query]
	[All<PositionComponent, CircleHitboxComponent>]
	[None<DyingMarkerComponent>]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void AddObjectsToGrid(
		[Data] in CenteredMovingUniformGrid<Entity> grid,
		Entity entity,
		ref PositionComponent pos
	)
	{
		if (!grid.ContainsWorld(pos.Position))
			return;

		var cell = grid.GetCellWorld(pos.Position);
		cell?.Add(entity);
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

	public int TryGetTargetsInArea(
		Vector2 areaCenter,
		float areaRadius,
		Span<Entity> targetBuffer,
		int hitLimit,
		bool recordTime = true
	)
	{
		using var rec = recordTime ? FrameTimes[0].Record() : null;
		if (targetBuffer.IsEmpty)
			return 0;

		if (areaRadius < 0f)
			return 0;

		var cellSize = _grid.CellSize;
		var topLeft = _grid.TopLeft;
		var dims = _grid.Dimensions;

		var minCellY = Math.Max(0, Mathf.FloorToInt((areaCenter.Y - areaRadius - topLeft.Y) / cellSize));
		var maxCellY = Math.Min(dims.Y - 1, Mathf.FloorToInt((areaCenter.Y + areaRadius - topLeft.Y) / cellSize));
		var radiusSq = areaRadius * areaRadius;
		var hitCount = 0;

		for (var cellY = minCellY; cellY <= maxCellY; cellY++)
		{
			var rowMinY = topLeft.Y + cellY * cellSize;
			var rowMaxY = rowMinY + cellSize;
			var dy =
				areaCenter.Y < rowMinY ? rowMinY - areaCenter.Y
				: areaCenter.Y > rowMaxY ? areaCenter.Y - rowMaxY
				: 0f;

			if (dy > areaRadius)
				continue;

			var dx = MathF.Sqrt(Math.Max(0f, radiusSq - dy * dy));
			var minCellX = Math.Max(0, Mathf.FloorToInt((areaCenter.X - dx - topLeft.X) / cellSize));
			var maxCellX = Math.Min(dims.X - 1, Mathf.FloorToInt((areaCenter.X + dx - topLeft.X) / cellSize));

			for (var cellX = minCellX; cellX <= maxCellX; cellX++)
			{
				var cell = _grid.GetCell(cellX, cellY);
				if (cell is null)
					continue;

				for (var i = 0; i < cell.Count; i++)
				{
					var entity = cell.Array[i];
					if (!GameWorld.World.IsAlive(entity))
						continue;
					if (!CircleHitTest(areaCenter, areaRadius, entity))
						continue;

					targetBuffer[hitCount++] = entity;
					if (hitCount >= hitLimit)
						return hitCount;
				}
			}
		}

		return hitCount;
	}

	public IEnumerable<Entity> GetTargetsInScreen()
	{
		for (var y = 0; y < _grid.Dimensions.Y; y++)
		{
			for (var x = 0; x < _grid.Dimensions.X; x++)
			{
				var cell = _grid.GetCell(x, y);
				if (cell is null)
					continue;
				for (var i = 0; i < cell.Count; i++)
				{
					var entity = cell.Array[i];
					if (!GameWorld.World.IsAlive(entity))
						continue;

					yield return entity;
				}
			}
		}
	}

	private readonly HashSet<Vector2I> _cachedCellsOnRay = [];
	private readonly HashSet<Entity> _cachedSeenIds = [];

	// NOTE: NOT THREAD SAFE
	public int GetTargetsRayCast(Vector2 from, float angle, float width, Span<Entity> hitBuffer, int hitLimit = -1)
	{
		using var _ = FrameTime.Record();
		_cachedCellsOnRay.Clear();
		_cachedSeenIds.Clear();
		var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
		var rayLength = _grid.WindowSize.Length();
		var rayEnd = from + direction * rayLength;

		GetCellsAlongLine(from, rayEnd, _cachedCellsOnRay);

		var cellWidth = Mathf.Clamp(Mathf.FloorToInt(width / _grid.CellSize), 0, int.MaxValue);
		var hitCount = 0;
		foreach (var cellIdx in _cachedCellsOnRay)
		{
			for (var dx = -cellWidth; dx <= cellWidth; dx++)
			for (var dy = -cellWidth; dy <= cellWidth; dy++)
			{
				var checkCell = new Vector2I(cellIdx.X + dx, cellIdx.Y + dy);
				var cell = _grid.GetCell(checkCell.X, checkCell.Y);
				if (cell is null || cell.Count <= 0)
					continue;

				for (var i = 0; i < cell.Count; i++)
				{
					var entity = cell.Array[i];
					if (!GameWorld.World.IsAlive(entity))
						continue;
					if (!_cachedSeenIds.Add(entity))
						continue;

					if (!GameWorld.World.TryGet<PositionComponent>(entity, out var pos))
						continue;
					if (!GameWorld.World.TryGet<CircleHitboxComponent>(entity, out var hitbox))
						continue;

					var distSq = DistanceSquaredPointToRay(pos.Position, from, direction, rayLength);
					var radiusSum = width + hitbox.Radius;
					if (distSq <= radiusSum * radiusSum)
					{
						hitBuffer[hitCount] = entity;
						hitCount++;
						if (hitCount != -1 && hitCount >= hitLimit)
							goto Exit;
					}
				}
			}
		}

		Exit:
		return hitCount;
	}

	private void GetCellsAlongLine(Vector2 start, Vector2 end, HashSet<Vector2I> cells)
	{
		var startCell = _grid.GetCellWorld(start);
		if (startCell is null)
			return;

		var direction = (end - start).Normalized();
		var maxDist = (end - start).Length();
		var current = startCell.Index;
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

			if (_grid.GetCell(current.X, current.Y) is null)
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
