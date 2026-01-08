namespace Game.Core.ECS;

public struct HealthComponent(int maxHealth)
{
    public readonly int MaxHealth = maxHealth;
    public int Health
    {
        get;
        set => field = Math.Clamp(value, 0, MaxHealth);
    } = maxHealth;
}
