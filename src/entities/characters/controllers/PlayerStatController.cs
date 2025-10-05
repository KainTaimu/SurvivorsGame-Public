namespace SurvivorsGame.Entities.Characters;

public partial class PlayerStatController : Node
{
    [Signal]
    public delegate void ItemAddedEventHandler();

    [Export]
    private PackedScene _deathEffect;

    [Export]
    private Player _owner;

    [Export]
    public PlayerStats PlayerStats;

    public override void _EnterTree()
    {
        PlayerStats.Player = _owner;
    }

    public override void _Ready()
    {
        if (_owner is null)
        {
            Logger.LogError("Owner is not assigned!");
        }

        PlayerStats.Health = PlayerStats.MaxHealth;
    }

    private void RegenerateHealth()
    {
        if (!_owner.Alive)
        {
            return;
        }

        var sumHealth = (int)Math.Ceiling(PlayerStats.Health + PlayerStats.HealthRegenPerSecond);

        PlayerStats.Health = Math.Clamp(sumHealth, 0, PlayerStats.MaxHealth);
    }
}