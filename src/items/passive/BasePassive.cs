using Godot.Collections;
using SurvivorsGame.Entities.Characters;
using SurvivorsGame.Items.Offensive;
using SurvivorsGame.Systems;

namespace SurvivorsGame.Items.Passive;

public partial class BasePassive : BaseItem
{
    [Export]
    public BaseItemProperties Properties = new();

    [Export]
    public BasePassiveStats Stats = new();

    [Export]
    public Array<BasePassiveStats> Upgrades = [];

    public bool Applied { get; private set; }
    protected static Player Player => GameWorld.Instance.MainPlayer;
    private static PlayerStats PlayerStats => Player.StatController.PlayerStats;

    // BUG: Maintains level even if deleted
    public override void _ExitTree()
    {
        RevertAppliedStats();
    }

    public virtual void Enter()
    {
        ApplyStats();
    }

    public virtual void Apply(double delta)
    {
    }

    protected virtual void PostUpgrade()
    {
    }

    private void Upgrade(int newLevel)
    {
        var upgrade = Upgrades[newLevel].Duplicate(true) as BasePassiveStats;
        Properties.CurrentLevel += 1;

        RevertAppliedStats();
        Stats = upgrade;
        ApplyStats();

        Logger.LogDebug($"Upgraded {Properties.Name} to {Properties.CurrentLevel + 1}");
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

    private void ApplyStats()
    {
        PlayerStats.MaxHealth += Stats.Health;
        PlayerStats.MoveSpeed += Stats.MoveSpeed;
        PlayerStats.Defense += Stats.Defense;
        PlayerStats.CriticalChance += Stats.CriticalChanceMultiplier;
        PlayerStats.PickupRange += Stats.PickupRangeMultiplier;
        PlayerStats.HealthRegenPerSecond += Stats.HealthRegenPerSecond;

        if (Stats.StatMultipliers is null)
        {
            return;
        }

        PlayerStats.HealthMultiplier += Stats.StatMultipliers.HealthMultiplier;
        PlayerStats.MoveSpeedMultiplier += Stats.StatMultipliers.MoveMultiplier;
        PlayerStats.IncomingDamageMultiplier += Stats.StatMultipliers.IncomingDamageMultiplier;
        PlayerStats.OutgoingDamageMultiplier += Stats.StatMultipliers.DamageMultiplier;
        PlayerStats.CriticalChanceMultiplier += Stats.StatMultipliers.CriticalChanceMultiplier;
        PlayerStats.CriticalDamageMultiplier += Stats.StatMultipliers.CriticalDamageMultiplier;
        PlayerStats.AttackSpeedMultiplier += Stats.StatMultipliers.AttackSpeedMultiplier;
        PlayerStats.ProjectileMultiplier += Stats.StatMultipliers.ProjectileScaleMultiplier;
        PlayerStats.XpMultiplier += Stats.StatMultipliers.XpMultiplier;
    }

    private void RevertAppliedStats()
    {
        PlayerStats.MaxHealth -= Stats.Health;
        PlayerStats.MoveSpeed -= Stats.MoveSpeed;
        PlayerStats.Defense -= Stats.Defense;
        PlayerStats.CriticalChance -= Stats.CriticalChanceMultiplier;
        PlayerStats.PickupRange -= Stats.PickupRangeMultiplier;
        PlayerStats.HealthRegenPerSecond -= Stats.HealthRegenPerSecond;

        if (Stats.StatMultipliers is null)
        {
            return;
        }

        PlayerStats.HealthMultiplier -= Stats.StatMultipliers.HealthMultiplier;
        PlayerStats.MoveSpeedMultiplier -= Stats.StatMultipliers.MoveMultiplier;
        PlayerStats.IncomingDamageMultiplier -= Stats.StatMultipliers.IncomingDamageMultiplier;
        PlayerStats.OutgoingDamageMultiplier -= Stats.StatMultipliers.DamageMultiplier;
        PlayerStats.CriticalChanceMultiplier -= Stats.StatMultipliers.CriticalChanceMultiplier;
        PlayerStats.CriticalDamageMultiplier -= Stats.StatMultipliers.CriticalDamageMultiplier;
        PlayerStats.AttackSpeedMultiplier -= Stats.StatMultipliers.AttackSpeedMultiplier;
        PlayerStats.ProjectileMultiplier -= Stats.StatMultipliers.ProjectileScaleMultiplier;
        PlayerStats.XpMultiplier -= Stats.StatMultipliers.XpMultiplier;
    }
}