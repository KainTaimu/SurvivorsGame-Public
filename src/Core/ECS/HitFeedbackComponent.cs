namespace Game.Core.ECS;

public record struct HitFeedbackComponent()
{
	public required float HitTime
	{
		get;
		init
		{
			field = value;
			HitTimeLeft = value;
		}
	} = 0.5f;

	public float HitTimeLeft
	{
		get;
		set => field = Math.Clamp(value, 0f, float.MaxValue);
	}

	public int Damage;
	public bool IsCrit;
}
