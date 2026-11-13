// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Godot.Collections;

namespace Game.Levels.Controllers.Waves;

[GlobalClass]
public abstract partial class AbstractWave : Node
{
	[Signal]
	public delegate void OnWaveStartEventHandler();

	[Signal]
	public delegate void OnWaveEndEventHandler();

	[Export]
	public string WaveName { get; set; } = "";

	[ExportCategory("Spawning")]
	[Export]
	public AbstractWaveBlueprintCollection EnemyBlueprints = null!;

	[ExportCategory("Spawn time")]
	[Export]
	public double SpawnMinTime
	{
		get;
		set
		{
			if (value > SpawnMaxTime)
			{
				CustomLogger.LogWarning($"SpawnMinTime ({field}) clamped to SpawnMaxTime ({SpawnMaxTime})");
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
				CustomLogger.LogWarning($"SpawnMaxTime ({field}) clamped to SpawnMinTime ({SpawnMinTime})");
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

	[ExportCategory("Spawn batches")]
	[Export]
	public int SpawnBatchMin = 1;

	[Export]
	public int SpawnBatchMax = 1;

	[Export]
	public Curve? SpawnBatchCurveOverMaxTime;

	[ExportCategory("Completion Rewards")]
	[Export]
	public Array<AbstractWaveCompletionReward>? Rewards = [];

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

	protected void GiveRewards()
	{
		if (Rewards is null)
			return;
		foreach (var reward in Rewards)
			reward.GiveReward();
	}

	public override string ToString()
	{
		return $"Wave {Index + 1}";
	}
}
