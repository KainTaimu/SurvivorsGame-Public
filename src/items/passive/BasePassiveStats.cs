namespace SurvivorsGame.Items.Passive;

[GlobalClass]
public partial class BasePassiveStats : BaseItemStats
{
    // Addition unless otherwise stated
    [Export] public int Health;
    [Export] public float MoveSpeed;
    [Export] public float Defense;
    [Export] public float CriticalChanceMultiplier;
    [Export] public float PickupRangeMultiplier;
    [Export] public float HealthRegenPerSecond;
    [Export] public float InvincibilityTime;
    [Export] public BasePassiveStatsMultipliers StatMultipliers;
}