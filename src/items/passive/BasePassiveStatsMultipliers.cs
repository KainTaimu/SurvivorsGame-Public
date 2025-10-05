namespace SurvivorsGame.Items.Passive;

[GlobalClass]
public partial class BasePassiveStatsMultipliers : Resource
{
    [Export] public float HealthMultiplier;
    [Export] public float MoveMultiplier;
    [Export] public float IncomingDamageMultiplier;
    [Export] public float DamageMultiplier;
    [Export] public float CriticalChanceMultiplier;
    [Export] public float CriticalDamageMultiplier;
    [Export] public float AttackSpeedMultiplier;
    [Export] public float ProjectileScaleMultiplier;
    [Export] public float XpMultiplier;
}