// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Arch.Core;
using Game.Levels.Controllers;

namespace Game.Items.Offensive;

public abstract partial class BaseOffensive : BaseItem
{
	[Signal]
	public delegate void OnAttackEventHandler();

	[Signal]
	public delegate void OnEquippedEventHandler();

	[Signal]
	public delegate void OnUnequippedEventHandler();

	protected EnemyTargetQuery TargetQuery => EnemyTargetQuery.Instance;

	public virtual void Process(double delta) { }

	protected virtual void PostUpgrade(int newLevel) { }

	public bool TryHandleHit(Entity entity)
	{
		if (!GameWorld.World.IsAlive(entity))
			return false;

		HandleHit(entity);
		return true;
	}

	public void HandleHit(Entity entity)
	{
		HandleDamage(entity);
		HandleHitEffects(entity);
	}

	/// <summary> Handle the damage to the enemy </summary>
	// ReSharper disable once InconsistentNaming
	protected abstract void HandleDamage(Entity entity);

	/// <summary> Handle additional effects to the enemy like knockback </summary>
	// ReSharper disable once InconsistentNaming
	protected abstract void HandleHitEffects(Entity entity);
}
