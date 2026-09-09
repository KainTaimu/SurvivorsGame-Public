namespace Game.Levels.Controllers.Waves;

[GlobalClass]
public abstract partial class AbstractWaveBlueprintCollection : Resource
{
	public abstract int Count { get; }
	public abstract EnemyBlueprint? GetBlueprint();
}