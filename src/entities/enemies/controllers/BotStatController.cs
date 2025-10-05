using SurvivorsGame.VFX;

namespace SurvivorsGame.Entities.Enemies;

public partial class BotStatController : Node
{
    [Export]
    private PackedScene _damageIndicator;

    [ExportCategory("Components")]
    [Export]
    private BaseEnemy _owner;

    public float Defense;

    public float Health = 1;

    [ExportCategory("Main attributes")]
    [Export]
    public int MaxDamage = 5;

    [Export]
    public float MaxDefense;

    [Export]
    public int MaxHealth = 100;

    [Export]
    public float MaxMoveSpeed = 150;

    public float MoveSpeed;

    [Export]
    public int XpGain = 10;

    public override void _Ready()
    {
        if (_owner == null)
        {
            Logger.LogError($"[ERROR] {GetParent().Name}'s StatController has no owner!");
            return;
        }

        Health = MaxHealth;
        MoveSpeed = MaxMoveSpeed;
        Defense = MaxDefense;
    }

    public void Damage(int damage)
    {
        var damageIndicator = _damageIndicator.Instantiate<DamageIndicator>();
        GetTree().Root.AddChild(damageIndicator);
        damageIndicator.ShowIndicator(_owner, damage);
    }
}