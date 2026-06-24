using Arch.Core;

namespace Game.Levels.Controllers;

[GlobalClass]
public partial class WaveSpawnCount : AbstractWave, IEnemyWave
{
	[Export]
	public int SpawnCountTarget = 30;

	public override void Process(double delta)
	{
		SpawnTimeLeft -= delta;

		if (SpawnedEntities.Count >= SpawnCountTarget)
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
		if (SpawnedEntities.Count >= SpawnCountTarget)
			return;

		SpawnTimeLeft = GetRandomSpawnTime();

		var bp = EnemyBlueprints.GetBlueprint();
		var id = Spawner.SpawnEnemy(bp);
		if (id is null)
		{
			Logger.LogError("failed to spawn");
			return;
		}

		SpawnedEntities.Add((Entity)id);
	}

	public override string ToString()
	{
		return $"Wave {Index} : {SpawnCountTarget} spawn count: " + $"{EnemyBlueprints
			.Count} types";
	}
}
