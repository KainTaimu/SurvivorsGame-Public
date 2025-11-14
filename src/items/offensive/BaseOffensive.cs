using Godot.Collections;
using SurvivorsGame.Entities.Characters;
using SurvivorsGame.Systems;

namespace SurvivorsGame.Items.Offensive;

public partial class BaseOffensive : BaseItem
{
    [Export]
    public BaseItemProperties Properties = new();

    [Export]
    public BaseOffensiveStats Stats = new();

    [Export]
    public Array<BaseOffensiveStats> Upgrades = [];

    public bool Enabled { get; private set; }
    protected static Player Player => GameWorld.Instance.MainPlayer;
    protected static PlayerStats PlayerStats =>
        GameWorld.Instance.MainPlayer.StatController.PlayerStats;

    public virtual void Initialize()
    {
        Enabled = true;
    }

    protected virtual void Attack() { }

    protected virtual void PostUpgrade(int newLevel) { }

    protected void Upgrade(int newLevel)
    {
        var upgrade = Upgrades[newLevel];
        Properties.CurrentLevel += 1;
        Logger.LogDebug($"Upgraded {Properties.Name} to {Properties.CurrentLevel + 1}");
        Stats = upgrade;
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
