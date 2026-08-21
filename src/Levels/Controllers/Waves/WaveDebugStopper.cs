namespace Game.Levels.Controllers;

[GlobalClass]
public partial class WaveDebugStopper : AbstractWave
{
	public override void Initialize(EnemyWaveController waveController)
	{
		WaveController = waveController;
	}

	public override void Process(double delta)
	{
		if (!WaveController.Enabled)
			return;
		WaveController.Enabled = true;
		EndWave();
	}

	public override void StartWave(int waveIndex)
	{
		WaveController.Enabled = false;
	}

	public override void EndWave()
	{
		EmitSignalOnWaveEnd();
	}

	public override void SpawnEnemy()
	{
		throw new NotImplementedException();
	}
}
