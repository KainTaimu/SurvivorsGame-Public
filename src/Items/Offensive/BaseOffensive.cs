using Arch.Core;
using Game.Levels.Controllers;
using Godot.Collections;

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
