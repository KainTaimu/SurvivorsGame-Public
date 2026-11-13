// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Game.Items.Projectiles;

namespace Game.Items.Offensive;

[GlobalClass]
public partial class SimpleBulletAttack : AbstractProjectileAttack
{
	public override BaseProjectile Attack(
		Func<BaseProjectile> getProjectile,
		Node? origin,
		Vector2 position,
		float rotation,
		float radius,
		float speed,
		int pierceLimit,
		Vector2 spriteScale
	)
	{
		var projectile = getProjectile();

		projectile.SetScale(spriteScale);
		projectile.SetPosition(position);
		projectile.Rotation = rotation;

		projectile.Initialize(origin, radius, speed, pierceLimit);
		return projectile;
	}
}
