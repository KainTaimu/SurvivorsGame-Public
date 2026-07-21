namespace Game.Core.Extensions;

public static class Vector2Extensions
{
	extension(Vector2 v)
	{
		public float GetLargestComponent()
		{
			return Mathf.Max(v.X, v.Y);
		}
	}
}
