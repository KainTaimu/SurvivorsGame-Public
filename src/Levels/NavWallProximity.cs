// Written by AI :/

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Game.Levels;

/// <summary>
/// Static "close to a wall" lookup baked once from the navmesh. A
/// cell is marked when it is outside the navmesh or within the clamp
/// margin of a boundary edge - the only places where
/// <see cref="NavigationServer2D.MapGetClosestPoint"/> can actually
/// move a point. The collision solvers use this to skip the per-enemy
/// server query for entities in open space.
/// </summary>
public class NavWallProximity
{
	private const int CELL_SIZE = 64;

	private byte[] _cells = [];
	private Vector2 _topLeft;
	private Vector2I _dims;

	// Written as the last build step, read by parallel solver queries.
	private volatile bool _ready;

	public void Build(IReadOnlyList<NavigationRegion2D> regions, float margin)
	{
		var (boundaryEdges, polygons) = GatherNavmeshGeometry(regions);
		if (boundaryEdges.Count == 0)
		{
			Logger.LogError(
				"NavWallProximity: no boundary edges found;",
				"clamp pre-filter disabled (NeedsClamp always true)."
			);
			return;
		}

		AllocateGrid(boundaryEdges, margin);
		RasterizeBoundaryEdges(boundaryEdges, margin);
		MarkOutsideCells(polygons);

		_ready = true;
		Logger.LogDebug("NavWallProximity: baked ", _dims, " cells, ", boundaryEdges.Count, " boundary edges.");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool NeedsClamp(Vector2 pos)
	{
		if (!_ready)
			return true;

		var x = Mathf.FloorToInt((pos.X - _topLeft.X) / CELL_SIZE);
		var y = Mathf.FloorToInt((pos.Y - _topLeft.Y) / CELL_SIZE);
		if ((uint)x >= (uint)_dims.X || (uint)y >= (uint)_dims.Y)
			return true;

		return _cells[y * _dims.X + x] != 0;
	}

	private static (List<(Vector2 A, Vector2 B)> Edges, List<Vector2[]> Polys) GatherNavmeshGeometry(
		IReadOnlyList<NavigationRegion2D> regions
	)
	{
		var edgeUses = new Dictionary<(long, long), int>();
		var edges = new List<(Vector2, Vector2)>();
		var polygons = new List<Vector2[]>();

		foreach (var region in regions)
		{
			var xform = region.GlobalTransform;
			var navPoly = region.NavigationPolygon;
			if (navPoly is null)
				continue;

			var local = navPoly.GetVertices();
			var world = new Vector2[local.Length];
			for (var i = 0; i < local.Length; i++)
				world[i] = xform * local[i];

			for (var p = 0; p < navPoly.GetPolygonCount(); p++)
			{
				var indices = navPoly.GetPolygon(p);
				var poly = new Vector2[indices.Length];
				for (var i = 0; i < indices.Length; i++)
					poly[i] = world[indices[i]];
				polygons.Add(poly);

				for (var i = 0; i < poly.Length; i++)
				{
					var a = poly[i];
					var b = poly[(i + 1) % poly.Length];
					edges.Add((a, b));
					var key = EdgeKey(a, b);
					edgeUses[key] = edgeUses.GetValueOrDefault(key) + 1;
				}
			}
		}

		// Edges shared by two baked polygons are interior seams, not
		// walls. Only unpaired edges border unwalkable space.
		var boundary = new List<(Vector2 A, Vector2 B)>();
		foreach (var (a, b) in edges)
		{
			if (edgeUses[EdgeKey(a, b)] == 1)
				boundary.Add((a, b));
		}

		return (boundary, polygons);
	}

	private void AllocateGrid(List<(Vector2 A, Vector2 B)> edges, float margin)
	{
		var min = new Vector2(float.MaxValue, float.MaxValue);
		var max = new Vector2(float.MinValue, float.MinValue);
		foreach (var (a, b) in edges)
		{
			min = min.Min(a).Min(b);
			max = max.Max(a).Max(b);
		}

		var pad = margin + CELL_SIZE;
		_topLeft = min - Vector2.One * pad;
		var size = max - min + Vector2.One * (pad * 2f);
		_dims = new Vector2I(Mathf.CeilToInt(size.X / CELL_SIZE), Mathf.CeilToInt(size.Y / CELL_SIZE));
		_cells = new byte[_dims.X * _dims.Y];
	}

	private void RasterizeBoundaryEdges(List<(Vector2 A, Vector2 B)> edges, float margin)
	{
		// Conservative radius: any point within `margin` of an edge is
		// guaranteed to land in a stamped cell; the extra CELL_SIZE
		// absorbs the sample spacing and the cell-center distance.
		var radius = margin + CELL_SIZE;
		var radiusSq = radius * radius;
		foreach (var (a, b) in edges)
		{
			var steps = Math.Max(1, Mathf.CeilToInt(a.DistanceTo(b) / (CELL_SIZE * 0.5f)));
			for (var s = 0; s <= steps; s++)
			{
				var p = a.Lerp(b, s / (float)steps);
				StampBrush(p, radius, radiusSq);
			}
		}
	}

	private void StampBrush(Vector2 p, float radius, float radiusSq)
	{
		var minX = Mathf.FloorToInt((p.X - radius - _topLeft.X) / CELL_SIZE);
		var minY = Mathf.FloorToInt((p.Y - radius - _topLeft.Y) / CELL_SIZE);
		var maxX = Mathf.FloorToInt((p.X + radius - _topLeft.X) / CELL_SIZE);
		var maxY = Mathf.FloorToInt((p.Y + radius - _topLeft.Y) / CELL_SIZE);

		for (var y = Math.Max(minY, 0); y <= Math.Min(maxY, _dims.Y - 1); y++)
		for (var x = Math.Max(minX, 0); x <= Math.Min(maxX, _dims.X - 1); x++)
		{
			var center = _topLeft + new Vector2(x + 0.5f, y + 0.5f) * CELL_SIZE;
			if (center.DistanceSquaredTo(p) <= radiusSq)
				_cells[y * _dims.X + x] = 1;
		}
	}

	private void MarkOutsideCells(List<Vector2[]> polygons)
	{
		// Cells outside the navmesh must stay clamped: an entity can
		// spawn deep inside a thick wall, beyond the margin band.
		var inside = new byte[_cells.Length];
		foreach (var poly in polygons)
			FillPolygon(poly, inside);

		for (var i = 0; i < _cells.Length; i++)
		{
			if (inside[i] == 0)
				_cells[i] = 1;
		}
	}

	private void FillPolygon(Vector2[] poly, byte[] inside)
	{
		var minY = float.MaxValue;
		var maxY = float.MinValue;
		foreach (var v in poly)
		{
			minY = Math.Min(minY, v.Y);
			maxY = Math.Max(maxY, v.Y);
		}

		var row0 = Math.Max(0, Mathf.FloorToInt((minY - _topLeft.Y) / CELL_SIZE));
		var row1 = Math.Min(_dims.Y - 1, Mathf.FloorToInt((maxY - _topLeft.Y) / CELL_SIZE));

		// Even-odd scanline fill of cell centers. Baked polygons are
		// convex, so rows have at most one crossing pair.
		var crossings = new float[poly.Length];
		for (var y = row0; y <= row1; y++)
		{
			var centerY = _topLeft.Y + (y + 0.5f) * CELL_SIZE;
			var count = 0;
			for (var i = 0; i < poly.Length; i++)
			{
				var a = poly[i];
				var b = poly[(i + 1) % poly.Length];
				if ((a.Y <= centerY) == (b.Y <= centerY))
					continue;
				var t = (centerY - a.Y) / (b.Y - a.Y);
				crossings[count++] = a.X + t * (b.X - a.X);
			}

			if (count < 2)
				continue;

			Array.Sort(crossings, 0, count);
			for (var i = 0; i + 1 < count; i += 2)
			{
				var col0 = Math.Max(0, Mathf.CeilToInt((crossings[i] - _topLeft.X) / CELL_SIZE - 0.5f));
				var col1 = Math.Min(_dims.X - 1, Mathf.FloorToInt((crossings[i + 1] - _topLeft.X) / CELL_SIZE - 0.5f));
				for (var x = col0; x <= col1; x++)
					inside[y * _dims.X + x] = 1;
			}
		}
	}

	private static (long, long) EdgeKey(Vector2 a, Vector2 b)
	{
		var ka = Pack(a);
		var kb = Pack(b);
		return ka < kb ? (ka, kb) : (kb, ka);
	}

	private static long Pack(Vector2 v)
	{
		var x = (long)Mathf.RoundToInt(v.X * 100f);
		var y = (long)Mathf.RoundToInt(v.Y * 100f);
		return (x << 32) | (y & 0xFFFFFFFFL);
	}
}
