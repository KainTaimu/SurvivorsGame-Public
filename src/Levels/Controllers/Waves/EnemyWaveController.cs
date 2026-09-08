using System.Collections.Generic;
using System.Linq;
using Arch.Core;
using Godot.Collections;

namespace Game.Levels.Controllers.Waves;

[GlobalClass]
public partial class EnemyWaveController : Node
{
	[Signal]
	public delegate void OnWaveStartEventHandler(AbstractWave wave);

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

	public AbstractWave? CurrentWave { get; private set; }
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
		CurrentWave?.Process(delta);
	}

	private void StartInitialWave()
	{
		CurrentWave = Waves.FirstOrDefault();
		if (CurrentWave is null)
		{
			ProcessMode = ProcessModeEnum.Disabled;
			return;
		}

		StartCurrentWave(CurrentWave);
	}

	public void NextWave()
	{
		CurrentWave?.OnWaveEnd -= NextWave;
		CurrentWave?.OnWaveEnd -= EmitSignalOnWaveEnd;

		if (CurrentWaveIndex + 1 >= Waves.Count)
		{
			CurrentWave = null;
			CustomLogger.LogDebug("Waves finished");
			return;
		}

		CurrentWaveIndex++;

		CurrentWave = Waves[CurrentWaveIndex];
		StartCurrentWave(CurrentWave);
	}

	private void StartCurrentWave(AbstractWave wave)
	{
		wave.OnWaveEnd += NextWave;
		wave.OnWaveEnd += EmitSignalOnWaveEnd;
		wave.StartWave(CurrentWaveIndex);
		EmitSignalOnWaveStart(wave);
	}

	private float GetWaveProgress()
	{
		if (CurrentWave is not IWaveProgress progress)
			return -1;
		return progress.Progress;
	}
}
