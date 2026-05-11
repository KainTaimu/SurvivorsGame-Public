using Godot.Collections;

namespace Game.Levels.Controllers;

[GlobalClass]
public partial class SimpleWaveBlueprintCollection : AbstractWaveBlueprintCollection
{
	[Export]
	public Array<EnemyBlueprint> EnemyBlueprints = null!;

	public override int Count => EnemyBlueprints.Count;

	public override EnemyBlueprint GetBlueprint()
	{
		throw new NotImplementedException();
	}
}
