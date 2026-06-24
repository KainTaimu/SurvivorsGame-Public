using System.Collections.Generic;
using Arch.Core;

namespace Game.Levels.Controllers;

[GlobalClass]
public abstract partial class AbstractWave : Resource, IEnemyWave
{
	[Signal]
	public delegate void OnWaveStartEventHandler();

	[Signal]
	public delegate void OnWaveEndEventHandler();

	[ExportGroup("Base")]
	[Export]
	public AbstractWaveBlueprintCollection EnemyBlueprints = null!;

	[Export]
	public double SpawnMinTime
	{
		get;
		set
		{
			if (value > SpawnMaxTime)
			{
				Logger.LogWarning($"SpawnMinTime ({field}) clamped to SpawnMaxTime ({SpawnMaxTime})");
				field = SpawnMaxTime;
				return;
			}

			field = value;
		}
	} = 0.5;

	[Export]
	public double SpawnMaxTime
	{
		get;
		set
		{
			if (value < SpawnMinTime)
			{
				Logger.LogWarning($"SpawnMaxTime ({field}) clamped to SpawnMinTime ({SpawnMinTime})");
				field = SpawnMinTime;
				return;
			}

			field = value;
		}
	} = 1;

	[Export]
	public Curve? SpawnTimeCurveOverMaxTime;

	[Export]
	public int SpawnBatchMin = 1;

	[Export]
	public int SpawnBatchMax = 1;

	private float RandomSpawnTime =>
		(float)(
			GD.RandRange(SpawnMinTime, SpawnMaxTime)
			* (SpawnTimeCurveOverMaxTime?.Sample((float)(SpawnTimeLeft / SpawnMaxTime)) ?? 1f)
		);

	public HashSet<Entity> SpawnedEntities => WaveController.SpawnedEntities;

	public double SpawnTimeLeft;
	protected EnemyWaveController WaveController = null!;
	public int Index;

	public EnemySpawner? Spawner => EnemySpawner.Instance;

	public abstract void Process(double delta);

	public abstract void Initialize(EnemyWaveController waveController);

	public abstract void StartWave(int waveIndex);

	public abstract void EndWave();

	public abstract void SpawnEnemy();

	public virtual float GetRandomSpawnTime()
	{
		return RandomSpawnTime;
	}

	public override string ToString()
	{
		return $"Wave {Index}";
	}
}
