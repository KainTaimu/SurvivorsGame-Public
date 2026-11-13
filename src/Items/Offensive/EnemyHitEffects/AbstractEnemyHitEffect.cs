// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

using Arch.Core;

namespace Game.Items.Offensive.EnemyHitEffects;

[GlobalClass]
public abstract partial class AbstractEnemyHitEffect : Resource
{
	public abstract void ApplyEffect(Entity entity);
}
