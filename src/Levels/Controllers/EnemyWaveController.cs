using System.Collections.Generic;
using System.Linq;
using Game.Core.ECS;
using Godot.Collections;

namespace Game.Levels.Controllers;

public partial class EnemyWaveController : Node
{
	[Export]
	public Array<EnemyWave> Waves = null!;

	[Export]
	public EntityComponentStore EntityComponentStore = null!;

	[ExportCategory("Toggles")]
	[Export]
	public bool Enabled
	{
		get;
		set { field = value; }
	} = true;

	public int TotalSpawned
	{
		get;
		set
		{
			// Spawned should not be decremented because we rely on it to create unique ids
			field =
				Math.Clamp(value, field, EntityComponentStore.MAX_SIZE);
		}
	}

	public int Alive
	{
		get;
		set { field = Math.Clamp(value, 0, EntityComponentStore.MAX_SIZE); }
	}

	private EnemyWave? _currentWave;
	private int _currentWaveIndex;
	public readonly List<int> SpawnedIds = [];

	public override void _Ready()
	{
		EntityComponentStore.BeforeEntityUnregistered += (_) => Alive--;

		foreach (var wave in Waves)
			wave.Initialize(this);

		_currentWave = Waves.FirstOrDefault();
		if (_currentWave is null)
		{
			ProcessMode = ProcessModeEnum.Disabled;
			return;
		}
		_currentWave.OnWaveEnd += NextWave;
		_currentWave.StartWave(_currentWaveIndex);
	}

	public override void _Process(double delta)
	{
		if (!Enabled)
			return;
		_currentWave?.Process(delta);
	}

	public void NextWave()
	{
		_currentWave?.OnWaveEnd -= NextWave;

		if (_currentWaveIndex + 1 >= Waves.Count)
		{
			_currentWave = null;
			Logger.LogDebug("Waves finished");
			return;
		}
		_currentWaveIndex++;

		_currentWave = Waves[_currentWaveIndex];
		_currentWave.OnWaveEnd += NextWave;
		_currentWave.StartWave(_currentWaveIndex);
	}
}
