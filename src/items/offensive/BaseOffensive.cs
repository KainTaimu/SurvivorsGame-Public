using Godot.Collections;
using SurvivorsGame.Entities.Characters;
using SurvivorsGame.Entities.Enemies;
using SurvivorsGame.Items.Effects;
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

    protected virtual void HandleHit(BaseEnemy target)
    {
        var crit = CalculateCrit();
        BaseEffect damageEffectType;

        if (crit == 0f)
        {
            damageEffectType = new EffectDamage { EffectValue = Stats.Damage, EffectDuration = 0f };
        }
        else
        {
            damageEffectType = new EffectCritDamage
            {
                EffectValue = Stats.Damage + crit,
                EffectDuration = 0f,
            };
        }

        target.EmitSignal(nameof(BaseEnemy.EnemyHit), damageEffectType);

        foreach (var effect in Stats.ProjectileEffects)
        {
            target.EmitSignal(nameof(BaseEnemy.EnemyHit), effect.Duplicate(true));
        }
    }

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
