using Game.Core.ECS;
using Game.Levels.Controllers;
using Godot.Collections;

namespace Game.Items.Offensive;

public abstract partial class BaseOffensive : BaseItem
{
	[Signal]
	public delegate void OnStatsChangedEventHandler();

	[Signal]
	public delegate void OnStatUpgradesChangedEventHandler();

	[Signal]
	public delegate void OnAttackEventHandler();

	[Export]
	public BaseItemProperties Properties = null!;

	[Export]
	public BaseOffensiveStats Stats
	{
		get;
		set
		{
			if (
				field is not null
				&& field.IsConnected(
					Resource.SignalName.Changed,
					Callable.From(EmitSignalOnStatsChanged)
				)
			)
				field.Changed -= EmitSignalOnStatsChanged;

			field = value;
			value.Changed += EmitSignalOnStatsChanged;
		}
	} = null!;

	[Export]
	public Array<BaseOffensiveStats> Upgrades = [];

	protected EnemyTargetQuery TargetQuery => EnemyTargetQuery.Instance;
	protected EntityComponentStore ComponentStore =>
		EntityComponentStore.Instance;

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
	protected void HandleDamageECS(int id)
	{
		if (!ComponentStore.GetComponent<HealthComponent>(id, out var health))
			return;

		var crit = CalculateCrit();
		var newHealth = health.Health - Stats.Damage - crit;

		var hit = new HitFeedbackComponent()
		{
			HitTime = 0.5f,
			Damage = Stats.Damage + crit,
			IsCrit = crit > 0,
		};
		if (!ComponentStore.GetComponent<HitFeedbackComponent>(id, out var _))
			ComponentStore.RegisterComponent(id, hit);
		else
			ComponentStore.SetComponent(id, hit);

		ComponentStore.SetComponent(id, health with { Health = newHealth });
	}

	/// <summary> Handle additional effects to the enemy like knockback </summary>
	protected virtual void HandleHitECS(int id) { }

	// protected abstract void HandleHitNode(Node node);

	protected void Upgrade(int newLevel)
	{
		var upgrade = Upgrades[newLevel];
		Properties.CurrentLevel++;
		Logger.LogDebug(
			$"Upgraded {Properties.Name} to {Properties.CurrentLevel + 1}"
		);
		Stats = upgrade;
		PostUpgrade(newLevel);
	}

	public void TryUpgrade()
	{
		Logger.LogWarning("Attempted to upgrade past max level");
		var incrementLevel = Properties.CurrentLevel + 1;
		if (incrementLevel > Upgrades.Count)
		{
			return;
		}

		Upgrade(Properties.CurrentLevel);
	}

	protected float GetAttackSpeed()
	{
		return Stats.AttackSpeed * PlayerStats.AttackSpeedMultiplier;
	}

	protected int CalculateCrit()
	{
		var roll = GD.Randf();
		if (roll > Stats.CritChanceProportion)
		{
			return 0;
		}

		return (int)Math.Round(Stats.Damage * Stats.CritDamageMultiplier);
	}
}
