namespace Game.Items.Offensive;

[GlobalClass]
public partial class BloomStats : ItemStat
{
	[Export]
	public float BloomCoefficientDeg { get; set; }

	public float CalculateBloomRads()
	{
		var bloomRad = Mathf.DegToRad(BloomCoefficientDeg);
		return (float)((bloomRad / 2) * GD.Randfn(0f, 1f));
	}
}
