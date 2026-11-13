// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Game.Utils;

namespace Game.Levels.Controllers.Waves;

[GlobalClass]
public partial class WaveSpawnCount : AbstractWave, IWaveProgress, IWaveResettable
{
	[Export]
	public int SpawnCountTarget = 30;

	public float Progress => 1 - (float)_spawnedEntitiesCount / SpawnCountTarget;

	private int _spawnedEntitiesCount;

	public override void Process(double delta)
	{
		SpawnTimeLeft -= delta;

		if (_spawnedEntitiesCount >= SpawnCountTarget)
		{
			EndWave();
			return;
		}

		if (SpawnTimeLeft <= 0)
		{
			for (var i = 0; i < GetRandomSpawnBatchCount(); i++)
			{
				SpawnTimeLeft = GetRandomSpawnTime();
				LastSpawnTime = SpawnTimeLeft;

				SpawnEnemy();
			}
		}
	}

	public override void Initialize(EnemyWaveController waveController)
	{
		WaveController = waveController;
	}

	public override void StartWave(int waveIndex)
	{
		SpawnTimeLeft = SpawnMaxTime;
		Index = waveIndex;
		EmitSignalOnWaveStart();
		CustomLogger.LogDebug($"New {ToString()}");
	}

	public override void EndWave()
	{
		EmitSignalOnWaveEnd();
		GiveRewards();
	}

	public override void SpawnEnemy()
	{
		if (Spawner is null)
			return;

		var bp = EnemyBlueprints.GetBlueprint();
		if (bp is null)
			return;

		var id = Spawner.SpawnEnemy(
			bp,
			ViewportTools.GetPositionOutsideViewport(
				followViewportScale: false,
				spawnDirection: CardinalDirection.East | CardinalDirection.East
			)
		);
		if (id is null)
		{
			CustomLogger.LogError("failed to spawn");
			return;
		}

		_spawnedEntitiesCount++;
	}

	public void Reset()
	{
		_spawnedEntitiesCount = 0;
	}

	private float GetRandomSpawnTime()
	{
		return (float)
			Mathf.Clamp(
				GD.RandRange(SpawnMinTime, SpawnMaxTime * (SpawnTimeCurveOverMaxTime?.Sample(1 - Progress) ?? 1f)),
				SpawnMinTime,
				SpawnMaxTime
			);
	}

	private int GetRandomSpawnBatchCount()
	{
		return Mathf.CeilToInt(
			Mathf.Clamp(
				GD.RandRange(SpawnBatchMin, SpawnBatchMax * (SpawnBatchCurveOverMaxTime?.Sample(1 - Progress) ?? 1f)),
				SpawnBatchMin,
				SpawnBatchMax
			)
		);
	}

	public override string ToString()
	{
		return $"{base.ToString()} : {SpawnCountTarget} spawn count: " + $"{EnemyBlueprints
			.Count} types";
	}
}
