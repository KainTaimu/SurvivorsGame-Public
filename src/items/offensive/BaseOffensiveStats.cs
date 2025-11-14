using Godot.Collections;

namespace SurvivorsGame.Items.Offensive;

[GlobalClass]
public partial class BaseOffensiveStats : BaseItemStats
{
    [Export]
    public int Damage;

    [Export]
    public float CritDamageMultiplier;

    [Export]
    public float CritChanceProportion;

    [Export]
    public int ProjectileSpeed;

    [Export]
    public float ProjectileScaleMultiplier;

    [Export]
    public float AttackSpeed;

    [Export]
    public int PierceLimit = 1;

    [Export]
    public Array<BaseEffect> ProjectileEffects;
}

