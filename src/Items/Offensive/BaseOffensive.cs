using Game.Utils;
using Godot.Collections;

namespace Game.Items.Offensive;

public abstract partial class BaseOffensive : BaseItem
{
	[Signal]
	public delegate void OnStatsChangedEventHandler();

	[Signal]
	public delegate void OnStatUpgradesChangedEventHandler();

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
			HandleHitECS(id.Value);
		}
	}

	protected abstract void HandleHitECS(int id);

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
