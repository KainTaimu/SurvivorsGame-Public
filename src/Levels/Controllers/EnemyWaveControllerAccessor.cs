namespace Game.Levels.Controllers;

[GlobalClass]
public partial class EnemyWaveControllerAccessor : RefCounted
{
	private static EnemyWaveController? WaveController => EnemyWaveController.Instance;

	public int AliveEnemies => WaveController?.Alive ?? 0;
}