using Arch.Core;

namespace Game.Levels.Controllers.Waves;

[GlobalClass]
public partial class WaveDuration : AbstractWave, IWaveProgress
{
	[Export]
	public double Duration = 30;

	[Export]
	public uint MaxMobs = 50;

	public float Progress => (float)(_waveTimeLeft / Duration);

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
		_waveTimeLeft = Duration;
		SpawnTimeLeft = SpawnMaxTime;
		Index = waveIndex;
		EmitSignalOnWaveStart();
		Logger.LogDebug($"New {ToString()}");
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
		if (WaveController.Alive >= MaxMobs)
			return;

		var bp = EnemyBlueprints.GetBlueprint();
		var enemy = Spawner.SpawnEnemy(bp);
		if (enemy is null)
		{
			Logger.LogError("failed to spawn");
			return;
		}

		SpawnedEntities.Add((Entity)enemy);
	}

	private protected override void GiveRewards()
	{
		if (Rewards is null)
			return;
		foreach (var reward in Rewards)
			reward.GiveReward();
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
		return $"{base.ToString()} : {Duration}s duration: {EnemyBlueprints.Count}" + $" types";
	}
}
