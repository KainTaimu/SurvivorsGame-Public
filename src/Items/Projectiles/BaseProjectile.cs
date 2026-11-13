// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace Game.Items.Projectiles;

public abstract partial class BaseProjectile : Node2D, IProjectile
{
	[Signal]
	public delegate void OnEntityHitEventHandler(EntityObject entityObject);

	public Node? Origin { get; private set; }
	public float HitRadius { get; private set; } = 24f;
	public float ProjectileSpeed { get; private set; }
	public int PierceLimit { get; private set; }

	protected bool IsInitialized { get; set; }

	public void Initialize(Node? origin, float hitRadius, float speed, int pierceLimit)
	{
		Origin = origin;
		HitRadius = hitRadius;
		ProjectileSpeed = speed;
		PierceLimit = pierceLimit;
		IsInitialized = true;
		PostInitialization();
	}

	protected abstract void PostInitialization();
}
