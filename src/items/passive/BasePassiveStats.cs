namespace SurvivorsGame.Items.Passive;

[GlobalClass]
public partial class BasePassiveStats : BaseItemStats
{
    [Export]
    public float CriticalChanceMultiplier;

    [Export]
    public float Defense;

    // Addition unless otherwise stated
    [Export]
    public int Health;

    [Export]
    public float HealthRegenPerSecond;

    [Export]
    public float InvincibilityTime;

    [Export]
    public float MoveSpeed;

    [Export]
    public float PickupRangeMultiplier;

    [Export]
    public BasePassiveStatsMultipliers StatMultipliers;
}

