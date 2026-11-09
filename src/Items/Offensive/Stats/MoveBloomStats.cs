namespace Game.Items.Offensive;

[GlobalClass]
public partial class MoveBloomStats : ItemStat
{
	public double MoveTime
	{
		get;
		set => field = Math.Clamp(value, 0, MoveTimeToMaxBloom);
	}

	public double MoveTimeFactor => MoveTime / (MoveTimeToMaxBloom != 0 ? MoveTimeToMaxBloom : 1 / double.MaxValue);

	[Export]
	public double MoveTimeToMaxBloom { get; set; }

	[Export]
	public double MoveBloomGrowthRate { get; set; }

	[Export]
	public double MoveBloomShrinkRate { get; set; }

	[Export]
	public double MoveBloomMinDeg { get; set; }

	[Export]
	public double MoveBloomMaxDeg { get; set; }

	[Export]
	public int MoveDamageMin { get; set; }

	[Export]
	public int MoveDamageMax { get; set; }
}
