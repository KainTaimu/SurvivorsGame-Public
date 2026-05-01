namespace Game.Core.ECS;

public record struct HitFeedbackComponent()
{
	public required double HitTime
	{
		get;
		init
		{
			field = value;
			HitTimeLeft = value;
		}
	} = 0.5;

	public double HitTimeLeft
	{
		get;
		set => field = Math.Clamp(value, 0, double.MaxValue);
	}

	public int Damage;
	public bool IsCrit;
}
