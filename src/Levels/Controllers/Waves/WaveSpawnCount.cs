using Game.Utils;

namespace Game.Levels.Controllers.Waves;

[GlobalClass]
public partial class WaveSpawnCount : AbstractWave, IWaveProgress, IWaveResettable
{
	[Export]
	public int SpawnCountTarget = 30;

	public float Progress => 1 - (float)_spawnedEntitiesCount / SpawnCountTarget;

	private int _spawnedEntitiesCount;

	public override void Process(double delta)
	{
		SpawnTimeLeft -= delta;

		if (_spawnedEntitiesCount >= SpawnCountTarget)
		{
			EndWave();
			return;
		}

		if (SpawnTimeLeft <= 0)
		{
			for (var i = 0; i < GetRandomSpawnBatchCount(); i++)
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
		CustomLogger.LogDebug($"New {ToString()}");
	}

	public override void EndWave()
	{
		EmitSignalOnWaveEnd();
		GiveRewards();
	}

	public override void SpawnEnemy()
	{
		if (Spawner is null)
			return;

		var bp = EnemyBlueprints.GetBlueprint();
		if (bp is null)
			return;

		var id = Spawner.SpawnEnemy(bp, ViewportTools.GetPositionOutsideViewport(followViewportScale: false));
		if (id is null)
		{
			CustomLogger.LogError("failed to spawn");
			return;
		}

		_spawnedEntitiesCount++;
	}

	private protected override void GiveRewards()
	{
		if (Rewards is null)
			return;
		foreach (var reward in Rewards)
			reward.GiveReward();
	}

	public void Reset()
	{
		_spawnedEntitiesCount = 0;
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

	private int GetRandomSpawnBatchCount()
	{
		return Mathf.CeilToInt(
			Mathf.Clamp(
				GD.RandRange(SpawnBatchMin, SpawnBatchMax * (SpawnBatchCurveOverMaxTime?.Sample(1 - Progress) ?? 1f)),
				SpawnBatchMin,
				SpawnBatchMax
			)
		);
	}

	public override string ToString()
	{
		return $"{base.ToString()} : {SpawnCountTarget} spawn count: " + $"{EnemyBlueprints
			.Count} types";
	}
}