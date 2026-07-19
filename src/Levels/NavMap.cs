using Game.Levels.Controllers;
using Game.Models;

namespace Game.Levels;

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

	[Export]
	private bool DrawPaths
	{
		get;
		set
		{
			field = value;
			CallDeferred(MethodName.ClearDrawnPaths);
		}
	}

	private UniformGridWorld<Vector2[]> _grid = null!;
	public Rect2 GridVisibilityRect => _grid.WorldBounds;

	public static Rid Map { get; private set; }
	public static NavMap Instance { get; private set; } = null!;

	public double ProcessTime { get; private set; }

	private Vector2 _cachedPlayerPosition;

	public override void _Ready()
	{
		Map = GetNavigationMap();

		UpdateGrid();

		Instance = this;
	}

	public override void _Process(double delta)
	{
		_cachedPlayerPosition = GameWorld.Instance.MainPlayer.GlobalPosition;

		_grid.ClearAll();
		_grid.Recenter(_cachedPlayerPosition);

		if (DrawPaths)
			QueueRedraw();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Engine.GetPhysicsFrames() % 4 != 0)
			return;
		if (IsBaking())
			return;
		var start = Time.GetTicksMsec();
		BakeNavigationPolygon();
		ProcessTime = Time.GetTicksMsec() - start;
	}

	public Span<Vector2> GetNavLine(Vector2 pos)
	{
		if (!_grid.TryGetWorld(pos, out var paths, out _))
		{
			UpdateNavCell(pos);
			if (!_grid.TryGetWorld(pos, out paths, out _))
				return [];
		}

		for (var i = 1; i < paths.Length; i++)
		{
			var jitter = Vector2.One * GD.RandRange(-30, 30);
			paths[i] += jitter;
		}

		return paths.AsSpan();
	}

	private void UpdateNavGrid()
	{
		for (var x = 0; x < _grid.Dimensions.X; x++)
		for (var y = 0; y < _grid.Dimensions.Y; y++)
			UpdateNavCell(new Vector2(x, y));
	}

	private void UpdateNavCell(Vector2 position)
	{
		var playerPos = _cachedPlayerPosition;

		var cell = _grid.WorldToCell(position);
		if (!_grid.IsValidCell(cell.X, cell.Y))
			return;
		var origin = _grid.CellCenterWorld(cell.X, cell.Y);
		var points = NavigationServer2D.MapGetPath(Map, origin, playerPos, true);
		if (points.Length < 2)
			return;
		_grid.Add(cell.X, cell.Y, points);
	}

	private void UpdateGrid()
	{
		var viewport = GetViewport();
		var windowSize = viewport.GetVisibleRect().Size;
		_grid = new UniformGridWorld<Vector2[]>(GridSize, new Vector2(windowSize.X, windowSize.X) * RangeFactor);
	}

	private void ClearDrawnPaths()
	{
		QueueRedraw();
	}

	public override void _Draw()
	{
		if (!DrawPaths)
			return;
		DrawRect(GridVisibilityRect, Colors.Red, false, 2, true);

		for (var x = 0; x < _grid.Dimensions.X; x++)
		for (var y = 0; y < _grid.Dimensions.Y; y++)
		{
			if (!_grid.TryGet(x, y, out var paths, out _))
				continue;
			DrawPolyline(paths, Colors.Red, 2, true);
		}
	}
}
