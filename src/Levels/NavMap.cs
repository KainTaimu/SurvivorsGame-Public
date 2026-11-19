// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Generic;
using Game.Core.Extensions;
using Game.Levels.Controllers;
using Game.Models;

namespace Game.Levels;

[GlobalClass]
public partial class NavMap : NavigationRegion2D
{
	[Export]
	private byte GridSize
	{
		get;
		set
		{
			field = value;
			CallDeferred(MethodName.UpdateGrid);
		}
	} = 64;

	[Export]
	private float RangeFactor
	{
		get;
		set
		{
			field = value;
			CallDeferred(MethodName.UpdateGrid);
		}
	} = 1f;

	// World-space band around walls in which enemies still get the
	// MapGetClosestPoint clamp. Must exceed the maximum distance an
	// enemy can move (speed * dt) plus be pushed by the collision
	// solver in a single physics frame.
	[Export]
	private float _clampMargin = 128f;

	private CenteredMovingUniformGrid<Vector2[]> _grid = null!;
	public Rect2 GridVisibilityRect => _grid.WorldBounds;

	// Null until the async build runs; treat null as "always clamp".
	public NavWallProximity? WallProximity { get; private set; }

	public static Rid Map { get; private set; }
	public static NavMap Instance { get; private set; } = null!;

	private Vector2 _cachedPlayerPosition;
	private static PhysicsDirectSpaceState2D _cachedSpace = null!;

	public override void _Ready()
	{
		Map = GetNavigationMap();

		UpdateGrid();

		Instance = this;

		WallProximity = new NavWallProximity();
		WallProximity.Build(CollectRegions(), _clampMargin);
	}

	private List<NavigationRegion2D> CollectRegions()
	{
		var regions = new List<NavigationRegion2D>();
		var found = GetTree().Root.FindChildren("*", nameof(NavigationRegion2D), true, false);
		foreach (var node in found)
		{
			if (
				node is NavigationRegion2D { Enabled: true } region
				&& region.GetNavigationMap() == Map
				&& region.NavigationPolygon is not null
			)
				regions.Add(region);
		}

		return regions;
	}

	public override void _PhysicsProcess(double delta)
	{
		_cachedSpace = GetWorld2D().DirectSpaceState;

		_cachedPlayerPosition = GameWorld.Instance.MainPlayer.GlobalPosition;

		_grid.ClearGrid();

		_grid.Recenter(_cachedPlayerPosition);
	}

	public ReadOnlySpan<Vector2> GetNavLine(Vector2 pos)
	{
		var cell = _grid.GetCellWorld(pos);
		if (cell is null)
			return [];

		if (cell.Count == 0)
		{
			UpdateNavCell(pos);
			if (cell.Count == 0)
				return [];
		}

		return cell.Array[0].AsSpan();
	}

	private void UpdateNavGrid()
	{
		for (var x = 0; x < _grid.Dimensions.X; x++)
		for (var y = 0; y < _grid.Dimensions.Y; y++)
			UpdateNavCell(new Vector2(x, y));
	}

	private readonly PhysicsRayQueryParameters2D _cachedRayQuery = new();

	private void UpdateNavCell(Vector2 position)
	{
		var cell = _grid.GetCellWorld(position);
		if (cell is null)
			return;

		var playerPos = _cachedPlayerPosition;

		// if outside navmap
		// const float threshold = 5f;
		// if (position.DistanceSquaredTo(NavigationServer2D.MapGetClosestPoint(Map, position)) > threshold * threshold)
		// 	return _grid.Add(cell.X, cell.Y, [position, playerPos]);
		if (!GridVisibilityRect.HasPoint(position))
		{
			cell.Add([position, playerPos]);
			return;
		}

		// if clear path to player
		_cachedRayQuery.From = position;
		_cachedRayQuery.To = playerPos;

		if (_cachedSpace.IntersectRay(_cachedRayQuery).Count == 0)
		{
			cell.Add([position, playerPos]);
			return;
		}

		var origin = _grid.TopLeft + cell.Position + Vector2.One * (_grid.CellSize * 0.5f);

		var points = GetPathsFromServer(origin, playerPos);
		// var points = GetPathsFromQuery(origin, playerPos);

		if (points.Length < 2)
			return;
		cell.Add(points);
	}

	private Vector2[] GetPathsFromServer(Vector2 origin, Vector2 playerPos)
	{
		return NavigationServer2D.MapGetPath(Map, origin, playerPos, true);
	}

	private Vector2[] GetPathsFromQuery(Vector2 origin, Vector2 playerPos)
	{
		var result = new NavigationPathQueryResult2D();
		NavigationServer2D.QueryPath(
			new NavigationPathQueryParameters2D
			{
				Map = GetWorld2D().NavigationMap,
				SimplifyPath = true,
				StartPosition = origin,
				TargetPosition = playerPos,
				PathPostprocessing = NavigationPathQueryParameters2D.PathPostProcessing.Edgecentered,
			},
			result
		);
		return result.Path;
	}

	private void UpdateGrid()
	{
		var viewport = GetViewport();
		if (viewport is null)
			return;
		var cam = viewport.GetCamera2D();
		if (cam is null)
			return;
		var windowLength = viewport.GetVisibleRect().Size.X * (1 / cam.Zoom.GetLargestComponent());
		var windowSize = Vector2.One * windowLength;
		_grid = new CenteredMovingUniformGrid<Vector2[]>(GridSize, windowSize * RangeFactor);
	}
}
