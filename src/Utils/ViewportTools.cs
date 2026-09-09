using Game.Levels.Controllers;

namespace Game.Utils;

public static class ViewportTools
{
	public static Vector2 GetPositionOutsideViewport(
		float margin = 0,
		CardinalDirection? spawnDirection = null,
		bool followViewportScale = true
	)
	{
		var viewport = GameWorld.Instance.GetViewport().GetCamera2D();
		var center = viewport.GetScreenCenterPosition();
		var zoom = viewport.Zoom;
		var size = viewport.GetViewportRect().Size;
		if (followViewportScale)
			size /= zoom;

		var edge = spawnDirection is not null ? (int)spawnDirection : GD.RandRange(0, 3);
		return edge switch
		{
			0 => new Vector2(
				(float)GD.RandRange(center.X - size.X - margin, center.X + size.X + margin),
				center.Y - size.Y - margin
			),
			1 => new Vector2(
				(float)GD.RandRange(center.X - size.X - margin, center.X + size.X + margin),
				center.Y + size.Y + margin
			),
			2 => new Vector2(
				center.X - size.X - margin,
				(float)GD.RandRange(center.Y - size.Y - margin, center.Y + size.Y + margin)
			),
			3 => new Vector2(
				center.X + size.X + margin,
				(float)GD.RandRange(center.Y - size.Y - margin, center.Y + size.Y + margin)
			),
			_ => throw new Exception("selected edge is unsupported"),
		};
	}
}