namespace Game.Items.Offensive;

[GlobalClass]
public partial class RadialPushStats : ItemStat
{
	[Export]
	public float NearbyPushRadius { get; set; }

	[Export]
	public float NearbyPushAmount { get; set; }
}
