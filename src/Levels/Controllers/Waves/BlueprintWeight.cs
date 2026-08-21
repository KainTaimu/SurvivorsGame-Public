namespace Game.Levels.Controllers;

[GlobalClass]
public partial class BlueprintWeight : Resource
{
	[Export]
	public EnemyBlueprint Blueprint = null!;

	[Export]
	public float Weight = 1;
}
