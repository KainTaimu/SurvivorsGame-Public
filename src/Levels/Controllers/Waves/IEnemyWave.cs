namespace Game.Levels.Controllers.Waves;

public interface IEnemyWave
{
	void Initialize(EnemyWaveController waveController);
	void Process(double delta);
	void StartWave(int waveIndex);
	void EndWave();
}
