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

	[ExportGroup("Spawning")]
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

	/// <summary>
	/// Max domain must be 1
	/// </summary>
	[Export]
	public Curve? SpawnTimeCurveOverMaxTime;

	[Export]
	public int SpawnBatchMin = 1;

	[Export]
	public int SpawnBatchMax = 1;

	[Export]
	public Curve? SpawnBatchCurveOverMaxTime;

	[ExportGroup("Completion Rewards")]
	[Export]
	public int Reward;

	public HashSet<Entity> SpawnedEntities => WaveController.SpawnedEntities;

	public double LastSpawnTime;
	public double SpawnTimeLeft;
	protected EnemyWaveController WaveController = null!;
	public int Index { get; set; }

	public EnemySpawner? Spawner => EnemySpawner.Instance;

	public abstract void Process(double delta);

	public abstract void Initialize(EnemyWaveController waveController);

	public abstract void StartWave(int waveIndex);

	public abstract void EndWave();

	public abstract void SpawnEnemy();

	public override string ToString()
	{
		return $"Wave {Index}";
	}
}
