namespace Game.Levels.Controllers.Waves;

[GlobalClass]
public abstract partial class AbstractWaveCompletionReward : Resource
{
	public abstract void GiveReward();
}
