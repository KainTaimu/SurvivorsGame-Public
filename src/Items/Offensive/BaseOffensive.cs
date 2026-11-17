// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

using Arch.Core;
using Game.Levels.Controllers;

namespace Game.Items.Offensive;

public abstract partial class BaseOffensive : BaseItem
{
	[Signal]
	public delegate void OnAttackEventHandler();

	protected EnemyTargetQuery TargetQuery => EnemyTargetQuery.Instance;

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
