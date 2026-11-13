// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

using Game.Items.Projectiles;

namespace Game.Items.Offensive;

[GlobalClass]
public abstract partial class AbstractProjectileAttack : Resource, IProjectileAttack
{
	public abstract BaseProjectile Attack(
		Func<BaseProjectile> getProjectile,
		Node? origin,
		Vector2 position,
		float rotation,
		float radius,
		float speed,
		int pierceLimit,
		Vector2 spriteScale
	);
}
