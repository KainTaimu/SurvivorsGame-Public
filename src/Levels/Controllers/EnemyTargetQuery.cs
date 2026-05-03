/*
 * Commented methods are written by AI slop machine
*/

using System.Collections.Generic;
using Game.Core.ECS;
using Game.Models;

namespace Game.Levels.Controllers;

// BUG:
// Hit detection reliability falls as FPS drops.
// There are attempts to make it less severe by
// increase sample size in *AlongSegment methods but
// the issue persists.
public partial class EnemyTargetQuery : Node
{
	[Export]
	private EntityComponentStore _entities = null!;

	[Export]
	private EnemyRenderer _renderer = null!;

	[Export]
	private int _gridSize = 16;

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
		_grid = new CenteredMovingUniformGrid<int>(_gridSize, windowSize);
	}

	public override void _Process(double delta)
	{
		var player = GameWorld.Instance.MainPlayer;
		if (player is null)
			return;
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

	public bool TryGetTargetsInArea(
		Vector2 areaCenter,
		float areaRadius,
		out int[] targetIds
	)
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
				{
					continue;
				}
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
		return true;
	}

	public bool TryGetTargetsInAreaAlongSegment(
		Vector2 fromAreaCenter,
		Vector2 toAreaCenter,
		float areaRadius,
		out int[] targetIds
	)
	{
		// credit: https://www.redblobgames.com/grids/circle-drawing/
		var targets = new HashSet<int>();

		var delta = toAreaCenter - fromAreaCenter;
		var length = delta.Length();
		var step =
			_gridSize
			* (1 + Performance.GetMonitor(Performance.Monitor.TimeProcess));

		var sampleCount = Math.Max(10, Mathf.CeilToInt(length / step));

		for (var b = 0; b <= sampleCount; b++)
		{
			var t = sampleCount == 0 ? 0f : (float)b / sampleCount;
			var sample = fromAreaCenter.Lerp(toAreaCenter, t);

			var top = Math.Ceiling(sample.Y - areaRadius);
			var bottom = Math.Floor(sample.Y + areaRadius);
			for (var y = top; y <= bottom; y++)
			{
				var dy = y - sample.Y;
				var dx = Math.Sqrt(areaRadius * areaRadius - dy * dy);
				var left = Math.Ceiling(sample.X - dx);
				var right = Math.Floor(sample.X + dx);
				for (var x = left; x <= right; x++)
				{
					var cell = _grid.GetCellWorld(
						new Vector2((float)x, (float)y)
					);
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
		}

		targetIds = [.. targets];
		return true;
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
	/// Finds all targets intersecting swept segment corridor.
	/// </summary>
	/// <param name="from">Segment start in world coordinates.</param>
	/// <param name="to">Segment end in world coordinates.</param>
	/// <param name="hitRadius">Corridor radius around segment.</param>
	/// <param name="targetIds">Output entity ids.</param>
	/// <returns>
	/// <c>true</c> when at least one target lies within swept corridor.
	/// </returns>
	public bool TryGetTargetsAlongSegment(
		Vector2 from,
		Vector2 to,
		float hitRadius,
		out int[] targetIds
	)
	{
		var delta = to - from;
		var length = delta.Length();
		var step =
			_gridSize * Performance.GetMonitor(Performance.Monitor.TimeProcess);
		var sampleCount = Math.Max(100, Mathf.CeilToInt(length / step));

		var hitRadiusSq = hitRadius * hitRadius;
		var targets = new HashSet<int>();

		for (var i = 0; i <= sampleCount; i++)
		{
			var t = sampleCount == 0 ? 0f : (float)i / sampleCount;
			var sample = from.Lerp(to, t);

			var sampleCell = _grid.GetCellWorld(sample);
			if (sampleCell is null)
				continue;

			FindTargetsAlongSegmentInNeighborCells(
				sampleCell.Index,
				from,
				to,
				hitRadiusSq,
				targets
			);
		}

		if (targets.Count == 0)
		{
			targetIds = [];
			return false;
		}

		targetIds = [.. targets];
		return true;
	}

	/// <summary>
	/// Updates closest swept-hit candidate in 3x3 neighborhood around a cell.
	/// </summary>
	/// <param name="centerCell">Center cell index for neighborhood scan.</param>
	/// <param name="from">Segment start.</param>
	/// <param name="to">Segment end.</param>
	/// <param name="hitRadiusSq">Squared corridor radius threshold.</param>
	/// <param name="closestId">Best candidate id tracked by reference.</param>
	/// <param name="closestDistSq">
	/// Best candidate squared distance tracked by reference.
	/// </param>
	private void FindTargetsAlongSegmentInNeighborCells(
		Vector2I centerCell,
		Vector2 from,
		Vector2 to,
		float hitRadiusSq,
		HashSet<int> targets
	)
	{
		for (var x = -1; x <= 1; x++)
		for (var y = -1; y <= 1; y++)
		{
			var cell = _grid.GetCell(centerCell.X + x, centerCell.Y + y);
			if (cell is null)
				continue;

			FindTargetsAlongSegmentInCell(cell, from, to, hitRadiusSq, targets);
		}
	}

	/// <summary>
	/// Updates closest swept-hit candidate from one grid cell.
	/// </summary>
	/// <param name="cell">Cell containing candidate enemy ids.</param>
	/// <param name="from">Segment start.</param>
	/// <param name="to">Segment end.</param>
	/// <param name="hitRadiusSq">Squared corridor radius threshold.</param>
	/// <param name="closestId">Best candidate id tracked by reference.</param>
	/// <param name="closestDistSq">
	/// Best candidate squared distance tracked by reference.
	/// </param>
	private void FindTargetsAlongSegmentInCell(
		UniformGridCell<int> cell,
		Vector2 from,
		Vector2 to,
		float hitRadiusSq,
		HashSet<int> targets
	)
	{
		for (var i = 0; i < cell.Count; i++)
		{
			var id = cell.Array[i];
			if (targets.Contains(id))
				continue;
			if (!_entities.GetComponent<PositionComponent>(id, out var ipos))
				continue;

			var distSq = DistanceSquaredPointToSegment(ipos.Position, from, to);
			if (distSq > hitRadiusSq)
				continue;

			targets.Add(id);
		}
	}

	/// <summary>
	/// Computes squared distance from point to finite segment.
	/// </summary>
	/// <param name="point">Point to test.</param>
	/// <param name="start">Segment start.</param>
	/// <param name="end">Segment end.</param>
	/// <returns>Squared distance to nearest point on segment.</returns>
	private static float DistanceSquaredPointToSegment(
		Vector2 point,
		Vector2 start,
		Vector2 end
	)
	{
		var segment = end - start;
		var segmentLengthSq = segment.LengthSquared();
		if (segmentLengthSq <= 0.0001f)
			return point.DistanceSquaredTo(start);

		var t = (point - start).Dot(segment) / segmentLengthSq;
		t = Mathf.Clamp(t, 0f, 1f);

		var closest = start + segment * t;
		return point.DistanceSquaredTo(closest);
	}
}
