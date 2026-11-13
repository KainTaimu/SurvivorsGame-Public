// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

using Game.Levels.Controllers;

namespace Game.Items.Projectiles;

public interface IPooledProjectile
{
	ProjectilePool ProjectilePool { get; set; }
	void ReturnToPool();
}
