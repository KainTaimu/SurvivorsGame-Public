using Arch.Core;

namespace Game.Levels.Controllers;

[GlobalClass]
public partial class WaveSpawnCount : AbstractWave, IEnemyWave, IWaveProgress
{
	[Export]
	public int SpawnCountTarget = 30;

	public float Progress => (float)SpawnedEntities.Count / SpawnCountTarget;

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
			for (var i = 0; i < GD.RandRange(SpawnBatchMin, SpawnBatchMax); i++)
			{
				SpawnTimeLeft = GetRandomSpawnTime();
				LastSpawnTime = SpawnTimeLeft;

				SpawnEnemy();
			}
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

		var bp = EnemyBlueprints.GetBlueprint();
		var id = Spawner.SpawnEnemy(bp);
		if (id is null)
		{
			Logger.LogError("failed to spawn");
			return;
		}

		SpawnedEntities.Add((Entity)id);
	}

	private float GetRandomSpawnTime()
	{
		return (float)(
			Mathf.Clamp(
				GD.RandRange(SpawnMinTime, SpawnMaxTime * (SpawnTimeCurveOverMaxTime?.Sample(1 - Progress) ?? 1f)),
				SpawnMinTime,
				SpawnMaxTime
			)
		);
	}

	public override string ToString()
	{
		return $"Wave {Index} : {SpawnCountTarget} spawn count: " + $"{EnemyBlueprints
			.Count} types";
	}
}
