using Game.Levels.Controllers;
using Game.Models;

namespace Game.Levels;

public partial class NavMap : NavigationRegion2D
{
	[Export]
	private byte _gridSize
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

	private CenteredMovingUniformGrid<Vector2[]> _grid = null!;
	public Rect2 GridVisibilityRect => _grid.WorldBounds;

	public static Rid Map { get; private set; }
	public static NavMap Instance { get; private set; } = null!;

	public double ProcessTime { get; private set; }

	private Vector2 CachedPlayerPosition;

	public override void _Ready()
	{
		Map = GetNavigationMap();

		UpdateGrid();

		Instance = this;
	}

	public override void _Process(double delta)
	{
		CachedPlayerPosition = GameWorld.Instance.MainPlayer.GlobalPosition;

		_grid.ClearGrid();
		_grid.Recenter(CachedPlayerPosition);

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
		BakeNavigationPolygon(true);
		ProcessTime = Time.GetTicksMsec() - start;
	}

	public Span<Vector2> GetNavLine(Vector2 pos)
	{
		var cell = _grid.GetCellWorld(pos);
		if (cell is null)
			return [];
		if (cell.Count == 0)
			UpdateNavCell(pos);
		var paths = cell.Array[0];

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
		{
			UpdateNavCell(new Vector2(x, y));
		}
	}

	private void UpdateNavCell(Vector2 position)
	{
		var playerPos = CachedPlayerPosition;

		var cell = _grid.GetCellWorld(position);
		if (cell is null)
			return;
		var origin = _grid.TopLeft + (Vector2)cell.Position + Vector2.One * (_grid.CellSize * 0.5f);
		var points = NavigationServer2D.MapGetPath(Map, origin, playerPos, true);
		if (points.Length < 2)
			return;
		cell.Add(points);
	}

	private void UpdateGrid()
	{
		var viewport = GetViewport();
		var windowSize = viewport.GetVisibleRect().Size;
		_grid = new CenteredMovingUniformGrid<Vector2[]>(
			_gridSize,
			new Vector2(windowSize.X, windowSize.X) * RangeFactor
		);
	}

	private void ClearDrawnPaths()
	{
		QueueRedraw();
	}

	public override void _Draw()
	{
		if (!DrawPaths)
			return;
		DrawRect(GridVisibilityRect, Colors.Red, false, 2, antialiased: true);

		for (var x = 0; x < _grid.Dimensions.X; x++)
		for (var y = 0; y < _grid.Dimensions.Y; y++)
		{
			var cell = _grid.GetCell(x, y);
			if (cell is null)
				continue;
			if (cell.Count == 0)
				continue;
			DrawPolyline(cell.Array[0], Colors.Red, 2, antialiased: true);
		}
	}
}
