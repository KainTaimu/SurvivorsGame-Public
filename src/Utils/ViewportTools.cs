// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

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

		var edge = GD.RandRange(0, 3);
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
