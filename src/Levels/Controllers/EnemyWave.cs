using System.Collections.Generic;
using Game.Core.ECS;
using Godot.Collections;

namespace Game.Levels.Controllers;

[GlobalClass]
public abstract partial class EnemyWave : Resource, IEnemyWave
{
    [Signal]
    public delegate void OnWaveStartEventHandler();

    [Signal]
    public delegate void OnWaveEndEventHandler();

    [ExportGroup("Base")]
    [Export]
    public Array<EnemyBlueprint> EnemyBlueprints = null!;

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

    public List<int> SpawnedIds => WaveController.SpawnedIds;

    public double SpawnTimeLeft;
    protected EnemyWaveController WaveController = null!;
    protected EntityComponentStore Entities = null!;
    public int Index;

    public EnemySpawner? Spawner => EnemySpawner.Instance;

    public abstract void Process(double delta);

    public abstract void Initialize(EnemyWaveController waveController);

    public abstract void StartWave(int waveIndex);

    public abstract void EndWave();

    public abstract void SpawnEnemy();

    public override string ToString()
    {
        return $"Wave {Index} : {EnemyBlueprints.Count} types";
    }
}
