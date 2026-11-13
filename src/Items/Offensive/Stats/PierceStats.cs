namespace Game.Items.Offensive;

[GlobalClass]
public partial class PierceStats : ItemStat
{
	[Export]
	public int PierceLimit { get; set; } = 1;
}
