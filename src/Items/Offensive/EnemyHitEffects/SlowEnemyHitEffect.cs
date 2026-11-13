// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

using Arch.Core;

namespace Game.Items.Offensive.EnemyHitEffects;

[GlobalClass]
public partial class SlowEnemyHitEffect : AbstractEnemyHitEffect
{
	public float SlowMultiplier { get; set; }

	public override void ApplyEffect(Entity entity)
	{
		OffensiveEffects.ApplyVelocityMultiplier(entity, SlowMultiplier);
	}
}
