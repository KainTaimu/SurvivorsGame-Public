using System.Collections.Generic;
using Game.Core.ECS;
using Godot.Collections;

namespace Game.Levels.Controllers;

[GlobalClass]
public partial class EnemyWave : Resource, IEnemyWave
{
	[Signal]
	public delegate void OnWaveStartEventHandler();

	[Signal]
	public delegate void OnWaveEndEventHandler();

	[Export]
	public Array<EnemyBlueprint> EnemyBlueprints = null!;

	[Export]
	public double Duration = 30;

	[Export]
	public uint MaxMobs = 50;

	[Export]
	public double SpawnMinTime
	{
		get;
		set
		{
			if (value > SpawnMaxTime)
			{
				Logger.LogWarning(
					$"SpawnMinTime ({field}) clamped to SpawnMaxTime ({SpawnMaxTime})"
				);
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
				Logger.LogWarning(
					$"SpawnMaxTime ({field}) clamped to SpawnMinTime ({SpawnMinTime})"
				);
				field = SpawnMinTime;
				return;
			}
			field = value;
		}
	} = 1;

	[Export]
	public int SpawnBatchMin = 1;

	[Export]
	public int SpawnBatchMax = 1;

	public List<int> SpawnedIds => _waveController.SpawnedIds;

	public double SpawnTimeLeft;
	public double WaveTimeLeft;
	private EnemyWaveController _waveController = null!;
	private EntityComponentStore _entities = null!;
	private int _index;

	private EnemySpawner? Spawner => EnemySpawner.Instance;

	public void Process(double delta)
	{
		WaveTimeLeft -= delta;
		SpawnTimeLeft -= delta;

		if (WaveTimeLeft <= 0)
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

	public void Initialize(EnemyWaveController waveController)
	{
		_waveController = waveController;
		_entities = waveController.EntityComponentStore;
	}

	public void StartWave(int waveIndex)
	{
		WaveTimeLeft = Duration;
		SpawnTimeLeft = SpawnMaxTime;
		_index = waveIndex;
		EmitSignalOnWaveStart();
		Logger.LogDebug($"New {ToString()}");
	}

	public void EndWave()
	{
		EmitSignalOnWaveEnd();
	}

	public void SpawnEnemy()
	{
		if (Spawner is null)
			return;
		if (_waveController.Alive >= MaxMobs)
			return;

		SpawnTimeLeft = GD.RandRange(SpawnMinTime, SpawnMaxTime);

		var bp = EnemyBlueprints.PickRandom();
		var id = Spawner.SpawnEnemy(bp);
		if (id == -1)
		{
			Logger.LogError("failed to spawn");
			return;
		}

		_waveController.Alive++;
		SpawnedIds.Add(id);
	}

	public override string ToString()
	{
		return $"Wave {_index} : {Duration}s duration: {EnemyBlueprints.Count} types";
	}
}

public interface IEnemyWave
{
	void Initialize(EnemyWaveController waveController);
	void Process(double delta);
	void StartWave(int waveIndex);
	void EndWave();
}
