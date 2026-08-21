namespace Game.Levels.Controllers;

[GlobalClass]
public partial class EnemyWaveControllerAccessor : RefCounted
{
	public EnemyWaveController? WaveController => EnemyWaveController.Instance;

	public int AliveEnemies => WaveController?.Alive ?? 0;
}
