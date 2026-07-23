using Arch.Core;

namespace Game.Levels.Controllers;

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
		var enemy = Spawner.SpawnEnemy(bp);
		if (enemy is null)
		{
			Logger.LogError("failed to spawn");
			return;
		}

		SpawnedEntities.Add((Entity)enemy);
	}

	public override string ToString()
	{
		return $"Wave {Index} : {Duration}s duration: {EnemyBlueprints.Count}" + $" types";
	}
}
