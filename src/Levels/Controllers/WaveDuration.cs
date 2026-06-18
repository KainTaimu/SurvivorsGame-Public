namespace Game.Levels.Controllers;

[GlobalClass]
public partial class WaveDuration : AbstractWave
{
	[Export]
	public double Duration = 30;

	[Export]
	public uint MaxMobs = 50;

	private double _waveTimeLeft;

	public override void Process(double delta)
	{
		_waveTimeLeft -= delta;
		SpawnTimeLeft -= delta;

		if (_waveTimeLeft <= 0)
		{
			EndWave();
			return;
		}

		if (SpawnTimeLeft <= 0)
		{
			// csharpier-ignore
			for (var i = 0; i < GD.RandRange(SpawnBatchMin, SpawnBatchMax); i++)
				SpawnEnemy();
		}
	}

	public override void Initialize(EnemyWaveController waveController)
	{
		WaveController = waveController;
		Entities = waveController.EntityComponentStore;
	}

	public override void StartWave(int waveIndex)
	{
		_waveTimeLeft = Duration;
		SpawnTimeLeft = SpawnMaxTime;
		Index = waveIndex;
		EmitSignalOnWaveStart();
		Logger.LogDebug($"New {ToString()}");
	}

	public override void EndWave()
	{
		EmitSignalOnWaveEnd();
	}

	public override void SpawnEnemy()
	{
		if (Spawner is null)
			return;
		if (WaveController.Alive >= MaxMobs)
			return;

		SpawnTimeLeft = GetRandomSpawnTime();

		var bp = EnemyBlueprints.GetBlueprint();
		var id = Spawner.SpawnEnemy(bp);
		if (id == -1)
		{
			Logger.LogError("failed to spawn");
			return;
		}

		WaveController.Alive++;
		SpawnedIds.Add(id);
	}

	public override string ToString()
	{
		return $"Wave {Index} : {Duration}s duration: {EnemyBlueprints.Count}" + $" types";
	}
}
