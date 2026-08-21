using System.Collections.Generic;
using System.Linq;
using Arch.Core;
using Godot.Collections;

namespace Game.Levels.Controllers;

[GlobalClass]
public partial class EnemyWaveController : Node
{
	[Signal]
	public delegate void OnWaveStartEventHandler();

	[Signal]
	public delegate void OnWaveEndEventHandler();

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
	public int CurrentWaveIndex { get; private set; }

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

		CallDeferred(MethodName.StartInitialWave);
	}

	public override void _ExitTree()
	{
		Instance = null;
	}

	public override void _Process(double delta)
	{
		if (!Enabled)
			return;
		_currentWave?.Process(delta);
	}

	private void StartInitialWave()
	{
		_currentWave = Waves.FirstOrDefault();
		if (_currentWave is null)
		{
			ProcessMode = ProcessModeEnum.Disabled;
			return;
		}

		_currentWave.OnWaveEnd += NextWave;
		_currentWave.OnWaveEnd += EmitSignalOnWaveEnd;
		_currentWave.StartWave(CurrentWaveIndex);
		EmitSignalOnWaveStart();
	}

	public void NextWave()
	{
		_currentWave?.OnWaveEnd -= NextWave;
		_currentWave?.OnWaveEnd -= EmitSignalOnWaveEnd;

		if (CurrentWaveIndex + 1 >= Waves.Count)
		{
			_currentWave = null;
			Logger.LogDebug("Waves finished");
			return;
		}

		CurrentWaveIndex++;

		_currentWave = Waves[CurrentWaveIndex];
		_currentWave.OnWaveEnd += NextWave;
		_currentWave.OnWaveEnd += EmitSignalOnWaveEnd;
		_currentWave.StartWave(CurrentWaveIndex);
		EmitSignalOnWaveStart();
	}

	private float GetWaveProgress()
	{
		if (_currentWave is not IWaveProgress progress)
			return -1;
		return progress.Progress;
	}
}
