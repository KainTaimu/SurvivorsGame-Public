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

	[Export]
	public Array<BaseOffensiveStats> Upgrades = [];

	public BaseOffensiveStats OffensiveStats => (BaseOffensiveStats)Stats;

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
		HandleDamageECS(entity);
		HandleHitECS(entity);
	}

	/// <summary> Handle the damage to the enemy </summary>
	// ReSharper disable once InconsistentNaming
	protected virtual void HandleDamageECS(Entity entity)
	{
		OffensiveEffects.ApplyDamage(
			entity,
			OffensiveStats.Damage,
			CalculateCrit(),
			OffensiveStats.DamageVarianceMultiplier,
			PlayerStats.OutgoingDamage
		);
	}

	/// <summary> Handle additional effects to the enemy like knockback </summary>
	// ReSharper disable once InconsistentNaming
	protected abstract void HandleHitECS(Entity entity);

	protected int CalculateCrit()
	{
		var roll = GD.Randf();
		if (roll > OffensiveStats.CritChanceProportion)
			return 0;

		return Mathf.CeilToInt(OffensiveStats.Damage * OffensiveStats.CritDamageMultiplier);
	}
}
