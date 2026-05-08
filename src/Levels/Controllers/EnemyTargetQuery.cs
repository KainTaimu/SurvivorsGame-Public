/*
 * Commented methods are written by AI slop machine
 */

using System.Collections.Generic;
using System.Linq;
using Game.Core.ECS;
using Game.Models;

namespace Game.Levels.Controllers;

public partial class EnemyTargetQuery : Node
{
	[Export]
	private EntityComponentStore _entities = null!;

	[Export]
	private EnemyRenderer _renderer = null!;

	// BREAKING: Changing this value breaks Projectile radius of weapons
	private const int GRID_SIZE = 32;

	public CenteredMovingUniformGrid<int> Grid => _grid;

	private CenteredMovingUniformGrid<int> _grid = null!;

	public static EnemyTargetQuery Instance { get; private set; } = null!;

	public override void _EnterTree()
	{
		Instance = this;
		var viewport = GetViewport();
		if (viewport is null)
		{
			Logger.LogError("missing viewport.");
			return;
		}

		var windowSize = viewport.GetVisibleRect().Size * 1.2f;
		_grid = new CenteredMovingUniformGrid<int>(GRID_SIZE, windowSize);
	}

	public override void _Process(double delta)
	{
		var player = GameWorld.Instance.MainPlayer;
		var playerPos = player.GlobalPosition;

		_grid.ClearGrid();
		_grid.Recenter(playerPos);
		AddObjectsToGrid();
	}

	private void AddObjectsToGrid()
	{
		foreach (var (id, pos) in _entities.Query<PositionComponent>())
		{
			if (!_grid.ContainsWorld(pos.Position))
				continue;

			var cell = _grid.GetCellWorld(pos.Position);
			cell?.Add(id);
		}
	}

	public bool TryGetTargetsInArea(Vector2 areaCenter, float areaRadius, out int[] targetIds)
	{
		// credit: https://www.redblobgames.com/grids/circle-drawing/
		var targets = new List<int>();

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
				var cell = _grid.GetCellWorld(new Vector2((float)x, (float)y));
				if (cell is null)
					continue;

				for (var i = 0; i < cell.Count; i++)
				{
					var id = cell.Array[i];
					if (targets.Contains(id))
						continue;
					targets.Add(id);
				}
			}
		}

		targetIds = [.. targets];
		return targets.Count > 0;
	}

	public IEnumerable<int> GetTargetsInScreen()
	{
		for (var x = 0; x < _grid.Dimensions.X; x++)
		{
			for (var y = 0; y < _grid.Dimensions.Y; y++)
			{
				var cell = _grid.GetCell(x, y);
				if (cell is null)
					continue;
				for (var i = 0; i < cell.Count; i++)
					yield return cell.Array[i];
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
	/// <param name="targetIds">Output entity ids.</param>
	/// <returns>
	/// <c>true</c> when at least one target lies within raycast corridor.
	/// </returns>
	public bool GetTargetsRayCast(Vector2 from, float angle, float width, out int[] targetIds)
	{
		var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
		var rayLength = _grid.WindowSize.Length();
		var rayEnd = from + direction * rayLength;
		var widthSq = width * width;

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
				if (_grid.GetCell(checkCell.X, checkCell.Y) is not null)
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

		var targets = new List<int>();
		var seenIds = new HashSet<int>();

		foreach (var cellIndex in allCells)
		{
			var cell = _grid.GetCell(cellIndex.X, cellIndex.Y);
			if (cell is null)
				continue;

			for (var i = 0; i < cell.Count; i++)
			{
				var id = cell.Array[i];
				if (seenIds.Contains(id))
					continue;
				seenIds.Add(id);

				if (!_entities.GetComponent<PositionComponent>(id, out var pos))
					continue;

				if (DistanceSquaredPointToRay(pos.Position, from, direction, rayLength) <= widthSq)
					targets.Add(id);
			}
		}

		targetIds = [.. targets];
		return targets.Count > 0;
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
