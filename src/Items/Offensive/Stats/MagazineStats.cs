namespace Game.Items.Offensive;

[GlobalClass]
public partial class MagazineStats : ItemStat
{
	[Export]
	public int Capacity { get; set; }

	public int Count
	{
		get;
		set => field = Mathf.Clamp(value, 0, Capacity);
	}

	public override void _EnterTree()
	{
		Count = Capacity;
	}
}
