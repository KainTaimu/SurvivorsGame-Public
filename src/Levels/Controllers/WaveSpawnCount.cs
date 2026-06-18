namespace Game.Levels.Controllers;

[GlobalClass]
public partial class WaveSpawnCount : AbstractWave, IEnemyWave
{
	[Export]
	public int SpawnCountTarget = 30;

	public override void Process(double delta)
	{
		SpawnTimeLeft -= delta;

		if (SpawnedIds.Count >= SpawnCountTarget)
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
		if (SpawnedIds.Count >= SpawnCountTarget)
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
		return $"Wave {Index} : {SpawnCountTarget} spawn count: " + $"{EnemyBlueprints
				.Count} types";
	}
}
