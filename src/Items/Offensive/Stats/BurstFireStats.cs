namespace Game.Items.Offensive;

[GlobalClass]
public partial class BurstFireStats : ItemStat
{
	[Export]
	public int BurstCount { get; set; } = 3;

	[Export]
	public float TimeBetweenBurst { get; set; } = 0.25f;
}
