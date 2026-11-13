namespace Game.Items.Offensive;

[GlobalClass]
public partial class AttackStats : ItemStat
{
	[Export]
	public float AttackSpeed { get; set; } = 0.2f;
}
