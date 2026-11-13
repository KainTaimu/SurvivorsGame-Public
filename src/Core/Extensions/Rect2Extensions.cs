// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace Game.Core.Extensions;

public static class Rect2Extensions
{
	extension(Rect2 rect)
	{
		/// <summary>
		/// Returns a new Rect2 that is recentered to p
		/// </summary>
		/// <returns>A new <see cref="Rect2"/> instance with its position adjusted to center the rectangle.
		/// </returns>
		public Rect2 GetCenteredToPoint(Vector2 p, float scale = 1f)
		{
			var size = rect.Size * scale;

			var centeredPos = p - size * 0.5f;

			return new Rect2(centeredPos, size);
		}
	}
}
