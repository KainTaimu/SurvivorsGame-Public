namespace Game.Items.Offensive;

[GlobalClass]
public partial class PumpActionStats : ItemStat
{
	[Export]
	public int PelletCount { get; set; } = 1;
}
