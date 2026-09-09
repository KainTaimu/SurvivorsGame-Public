namespace Game.Levels.Controllers.Waves;

[GlobalClass]
public partial class EnemyWaveControllerAccessor : RefCounted
{
	public EnemyWaveController? WaveController => EnemyWaveController.Instance;

	public int AliveEnemies => EnemyTracker.EnemyCount;
}