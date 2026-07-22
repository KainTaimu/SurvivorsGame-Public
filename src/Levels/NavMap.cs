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

	private UniformGridWorld<Vector2[]> _grid = null!;
	public Rect2 GridVisibilityRect => _grid.WorldBounds;

	public static Rid Map { get; private set; }
	public static NavMap Instance { get; private set; } = null!;

	private Vector2 _cachedPlayerPosition;
	private static PhysicsDirectSpaceState2D _cachedSpace = null!;

	public override void _Ready()
	{
		Map = GetNavigationMap();

		UpdateGrid();

		Instance = this;
	}

	public override void _PhysicsProcess(double delta)
	{
		_cachedPlayerPosition = GameWorld.Instance.MainPlayer.GlobalPosition;
		_cachedSpace = GetWorld2D().DirectSpaceState;

		_grid.ClearAll();

		_grid.Recenter(_cachedPlayerPosition);
	}

	public Span<Vector2> GetNavLine(Vector2 pos)
	{
		var cellIndex = _grid.WorldToCell(pos);
		Vector2[] paths;

		// if stale cell
		if (_grid.GetCellCount(cellIndex.X, cellIndex.Y) > 0)
		{
			_grid.TryGetWorld(pos, out paths, out _);
			return paths.AsSpan();
		}

		var handle = UpdateNavCell(pos);
		if (handle is null)
			return [];

		_grid.TryGet(handle.Value, out paths);
		return paths.AsSpan();
	}

	private void UpdateNavGrid()
	{
		for (var x = 0; x < _grid.Dimensions.X; x++)
		for (var y = 0; y < _grid.Dimensions.Y; y++)
			UpdateNavCell(new Vector2(x, y));
	}

	private GridCellHandle? UpdateNavCell(Vector2 position)
	{
		var cell = _grid.WorldToCell(position);
		if (!_grid.IsValidCell(cell.X, cell.Y))
			return null;

		var playerPos = _cachedPlayerPosition;

		// if outside navmap
		const float threshold = 5f;
		if (position.DistanceSquaredTo(NavigationServer2D.MapGetClosestPoint(Map, position)) > threshold * threshold)
			return _grid.Add(cell.X, cell.Y, [position, playerPos]);

		// if clear path to player
		var query = new PhysicsRayQueryParameters2D { From = position, To = playerPos };
		if (_cachedSpace.IntersectRay(query).Count == 0)
			return _grid.Add(cell.X, cell.Y, [position, playerPos]);

		var origin = _grid.CellCenterWorld(cell.X, cell.Y);

		var points = GetPathsFromServer(origin, playerPos);
		// var points = GetPathsFromQuery(origin, playerPos);

		if (points.Length < 2)
			return null;
		return _grid.Add(cell.X, cell.Y, points);
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
		var windowSize = viewport.GetVisibleRect().Size * (1 / cam.Zoom.GetLargestComponent());
		_grid = new UniformGridWorld<Vector2[]>(GridSize, windowSize * RangeFactor, 1);
	}
}
