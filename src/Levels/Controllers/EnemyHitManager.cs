using System.Collections.Generic;
using Game.Core.ECS;
using Game.Models;

namespace Game.Levels.Controllers;

/// <summary>
/// Builds and queries a moving enemy hit grid.
/// </summary>
/// <remarks>
/// This manager keeps a centered <see cref="CenteredMovingUniformGrid{T}"/> that
/// follows player position each frame. Enemy entity ids are inserted from ECS
/// position data and then used by hit queries.
///
/// Query modes:
/// - <see cref="TryGetTarget"/>: point query in current cell.
/// - <see cref="TryGetTargetAlongSegment"/>: swept query with radius.
///
/// Swept query samples along movement segment and checks neighboring cells to
/// reduce tunneling for fast projectiles, including zero-length segments.
/// </remarks>
public partial class EnemyHitManager : Node
{
	/// <summary>
	/// ECS store that owns enemy components.
	/// </summary>
	[Export]
	private EntityComponentStore _entities = null!;

	/// <summary>
	/// Renderer reference for inspector wiring and debug workflows.
	/// </summary>
	[Export]
	private EntityRenderer _renderer = null!;

	/// <summary>
	/// Uniform grid cell size in world units.
	/// </summary>
	/// <remarks>
	/// Smaller values increase precision but may increase query overhead.
	/// Larger values reduce precision but may reduce bookkeeping work.
	/// </remarks>
	[Export]
	private int _gridSize = 16;

	/// <summary>
	/// Centered moving grid storing enemy ids.
	/// </summary>
	private CenteredMovingUniformGrid<int> _grid = null!;

	/// <summary>
	/// Singleton instance for global gameplay access.
	/// </summary>
	public static EnemyHitManager Instance { get; private set; } = null!;

	/// <summary>
	/// Initializes singleton and allocates spatial grid.
	/// </summary>
	/// <remarks>
	/// Grid window uses viewport size * 2 so nearby off-screen enemies remain
	/// queryable during fast movement and camera shifts.
	/// </remarks>
	public override void _EnterTree()
	{
		Instance = this;
		var viewport = GetViewport();
		if (viewport is null)
		{
			Logger.LogError("EnemyCollisionSolver: missing viewport.");
			return;
		}

		var windowSize = viewport.GetVisibleRect().Size * 1.2f;
		_grid = new CenteredMovingUniformGrid<int>(_gridSize, windowSize);
	}

	/// <summary>
	/// Rebuilds grid around player and handles debug click probing.
	/// </summary>
	/// <param name="delta">Frame delta time, currently unused.</param>
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

	/// <summary>
	/// Inserts all ECS position entries into current grid cells.
	/// </summary>
	/// <remarks>
	/// Caller must clear and recenter grid before invoking this method.
	/// </remarks>
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
		Logger.LogDebug(areaCenter);

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

	/// <summary>
	/// Finds closest target intersecting swept segment corridor.
	/// </summary>
	/// <param name="from">Segment start in world coordinates.</param>
	/// <param name="to">Segment end in world coordinates.</param>
	/// <param name="hitRadius">Corridor radius around segment.</param>
	/// <param name="targetId">
	/// Output entity id. Set to <c>-1</c> when no target found.
	/// </param>
	/// <returns>
	/// <c>true</c> when at least one target lies within swept corridor.
	/// </returns>
	/// <remarks>
	/// Segment gets sampled at <c>_gridSize * 0.5</c> spacing. For each sample, the
	/// algorithm checks sample cell and eight neighbors (3x3 neighborhood). Candidate
	/// ranking uses squared distance from enemy point to finite segment.
	/// </remarks>
	public bool TryGetTargetAlongSegment(
		Vector2 from,
		Vector2 to,
		float hitRadius,
		out int targetId
	)
	{
		targetId = -1;

		var delta = to - from;
		var length = delta.Length();
		var step = _gridSize * 0.5f;
		var sampleCount = Math.Max(1, Mathf.CeilToInt(length / step));
		var hitRadiusSq = hitRadius * hitRadius;

		var closestId = -1;
		var closestDistSq = float.PositiveInfinity;

		for (var i = 0; i <= sampleCount; i++)
		{
			var t = sampleCount == 0 ? 0f : (float)i / sampleCount;
			var sample = from.Lerp(to, t);

			var sampleCell = _grid.GetCellWorld(sample);
			if (sampleCell is null)
				continue;

			FindClosestAlongSegmentInNeighborCells(
				sampleCell.Index,
				from,
				to,
				hitRadiusSq,
				ref closestId,
				ref closestDistSq
			);
		}

		if (closestId == -1)
			return false;

		targetId = closestId;
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
	private void FindClosestAlongSegmentInNeighborCells(
		Vector2I centerCell,
		Vector2 from,
		Vector2 to,
		float hitRadiusSq,
		ref int closestId,
		ref float closestDistSq
	)
	{
		for (var x = -1; x <= 1; x++)
		for (var y = -1; y <= 1; y++)
		{
			var cell = _grid.GetCell(centerCell.X + x, centerCell.Y + y);
			if (cell is null)
				continue;

			FindClosestAlongSegmentInCell(
				cell,
				from,
				to,
				hitRadiusSq,
				ref closestId,
				ref closestDistSq
			);
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
	private void FindClosestAlongSegmentInCell(
		UniformGridCell<int> cell,
		Vector2 from,
		Vector2 to,
		float hitRadiusSq,
		ref int closestId,
		ref float closestDistSq
	)
	{
		for (var i = 0; i < cell.Count; i++)
		{
			var id = cell.Array[i];
			if (!_entities.GetComponent<PositionComponent>(id, out var ipos))
				continue;

			var distSq = DistanceSquaredPointToSegment(ipos.Position, from, to);
			if (distSq > hitRadiusSq)
				continue;

			if (distSq < closestDistSq)
			{
				closestDistSq = distSq;
				closestId = id;
			}
		}
	}

	/// <summary>
	/// Computes squared distance from point to finite segment.
	/// </summary>
	/// <param name="point">Point to test.</param>
	/// <param name="start">Segment start.</param>
	/// <param name="end">Segment end.</param>
	/// <returns>Squared distance to nearest point on segment.</returns>
	/// <remarks>
	/// Uses projection onto segment vector and clamps interpolation factor to
	/// [0, 1]. Squared distance avoids sqrt in hot loops.
	/// </remarks>
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
