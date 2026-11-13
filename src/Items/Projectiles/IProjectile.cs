// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Items.Projectiles;

public interface IProjectile
{
	float HitRadius { get; }
	float ProjectileSpeed { get; }
	int PierceLimit { get; }
}
