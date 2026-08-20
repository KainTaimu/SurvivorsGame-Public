using System.Collections.Generic;
using System.Linq;
using Arch.Core;
using Godot.Collections;

namespace Game.Levels.Controllers;

[GlobalClass]
public partial class EnemyWaveController : Node
{
	[Export]
	public Array<AbstractWave> Waves = null!;

	[ExportCategory("Toggles")]
	[Export]
	public bool Enabled
	{
		get;
		set => field = value;
	} = true;

	public int TotalSpawned { get; private set; }

	public float CurrentWaveProgress => GetWaveProgress();

	public int Alive => SpawnedEntities.Count;

	private AbstractWave? _currentWave;
	private int _currentWaveIndex;

	public readonly HashSet<Entity> SpawnedEntities = [];

	public static EnemyWaveController? Instance { get; private set; }

	public override void _Ready()
	{
		Instance = this;

		GameWorld.World.SubscribeEntityDestroyed(
			(in entity) =>
			{
				SpawnedEntities.Remove(entity);
			}
		);

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

	private float GetWaveProgress()
	{
		if (_currentWave is not IWaveProgress progress)
			return -1;
		return progress.Progress;
	}
}
