using Game.Core.ECS;
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

	protected EntityComponentStore ComponentStore => EntityComponentStore.Instance;

	public abstract void Attack();

	protected virtual void PostUpgrade(int newLevel) { }

	public void HandleHit(Node? node = null, int? id = null)
	{
		if (!(node is null ^ id is null))
		{
			// Logger.LogError("cannot pass both node and id");
			throw new ArgumentException("cannot pass both node and id");
		}

		if (node is not null)
		{
			// HandleHitNode(node);
			throw new NotImplementedException();
		}

		if (id is not null)
		{
			HandleDamageECS(id.Value);
			HandleHitECS(id.Value);
		}
	}

	/// <summary> Handle the damage to the enemy </summary>
	// ReSharper disable once InconsistentNaming
	protected void HandleDamageECS(int id)
	{
		if (!ComponentStore.GetComponent<HealthComponent>(id, out var health))
			return;

		var crit = CalculateCrit();
		var randomDamage =
			OffensiveStats.Damage > 1 ? Mathf.CeilToInt(GD.RandRange(-0.15, 0.15) * OffensiveStats.Damage) : 0;
		var damage = Mathf.CeilToInt(
			(OffensiveStats.Damage + crit + randomDamage) * PlayerStats.OutgoingDamageMultiplier
		);
		var newHealth = health.Health - damage;

		var hit = new HitFeedbackComponent
		{
			HitTime = 0.5f,
			Damage = damage,
			IsCrit = crit > 0,
		};
		if (!ComponentStore.GetComponent<HitFeedbackComponent>(id, out _))
			ComponentStore.RegisterComponent(id, hit);
		else
			ComponentStore.SetComponent(id, hit);

		ComponentStore.SetComponent(id, health with { Health = newHealth });
	}

	/// <summary> Handle additional effects to the enemy like knockback </summary>
	// ReSharper disable once InconsistentNaming
	protected virtual void HandleHitECS(int id) { }

	// protected abstract void HandleHitNode(Node node);

	protected void Upgrade(int newLevel)
	{
		var upgrade = Upgrades[newLevel];
		Properties.CurrentLevel++;
		Logger.LogDebug($"Upgraded {Properties.Name} to {Properties.CurrentLevel + 1}");
		Stats = upgrade;
		PostUpgrade(newLevel);
	}

	public void TryUpgrade()
	{
		Logger.LogWarning("Attempted to upgrade past max level");
		var incrementLevel = Properties.CurrentLevel + 1;
		if (incrementLevel > Upgrades.Count)
			return;

		Upgrade(Properties.CurrentLevel);
	}

	protected float GetAttackSpeed()
	{
		return OffensiveStats.AttackSpeed * PlayerStats.AttackSpeedMultiplier;
	}

	protected int CalculateCrit()
	{
		var roll = GD.Randf();
		if (roll > OffensiveStats.CritChanceProportion)
			return 0;

		return Mathf.CeilToInt(OffensiveStats.Damage * OffensiveStats.CritDamageMultiplier);
	}
}
