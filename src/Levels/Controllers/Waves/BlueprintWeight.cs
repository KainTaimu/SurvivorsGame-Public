namespace Game.Levels.Controllers.Waves;

[GlobalClass]
public partial class BlueprintWeight : Resource
{
	[Export]
	public EnemyBlueprint Blueprint = null!;

	[Export]
	public float Weight = 1;
}
