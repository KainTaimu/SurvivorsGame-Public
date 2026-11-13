// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Levels.Controllers.Waves;

[GlobalClass]
public abstract partial class AbstractWaveBlueprintCollection : Resource
{
	public abstract int Count { get; }
	public abstract EnemyBlueprint? GetBlueprint();
}
