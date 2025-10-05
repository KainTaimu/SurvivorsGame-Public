using Godot.Collections;

namespace SurvivorsGame.Items.Offensive;

[GlobalClass]
public partial class BaseOffensiveStats : BaseItemStats
{
    [Export]
    public float AttackSpeed;

    [Export]
    public float CritChanceProportion;

    [Export]
    public float CritDamageMultiplier;

    [Export]
    public int Damage;

    [Export]
    public int PierceLimit = 1;

    [Export]
    public Array<BaseEffect> ProjectileEffects;

    [Export]
    public float ProjectileScaleMultiplier;

    [Export]
    public int ProjectileSpeed;
}