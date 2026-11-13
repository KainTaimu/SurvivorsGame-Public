// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Levels.Controllers.Waves;

[GlobalClass]
public partial class EnemyWaveControllerAccessor : RefCounted
{
	public EnemyWaveController? WaveController => EnemyWaveController.Instance;

	public int AliveEnemies => EnemyTracker.EnemyCount;
}
