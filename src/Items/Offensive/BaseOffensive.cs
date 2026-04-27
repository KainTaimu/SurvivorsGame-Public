using Game.Levels.Controllers;
using Game.Players;
using Godot.Collections;

namespace Game.Items.Offensive;

public partial class BaseOffensive : BaseItem
{
	[Export]
	public BaseItemProperties Properties = new();

	[Export]
	public BaseOffensiveStats Stats = new();

	[Export]
	public Array<BaseOffensiveStats> Upgrades = [];

	public virtual void Initialize() { }

	protected virtual void Attack() { }

	protected virtual void PostUpgrade(int newLevel) { }

	protected virtual void HandleHit(Node target) { }

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

	protected float CalculateCrit()
	{
		var roll = GD.Randf();
		if (roll > Stats.CritChanceProportion)
		{
			return 0f;
		}

		return Stats.Damage * Stats.CritDamageMultiplier;
	}
}
