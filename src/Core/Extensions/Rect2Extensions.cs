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
