// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Levels.Controllers.Waves;

[GlobalClass]
public partial class BlueprintWeight : Resource
{
	[Export]
	public EnemyBlueprint Blueprint = null!;

	[Export]
	public float Weight = 1;
}
