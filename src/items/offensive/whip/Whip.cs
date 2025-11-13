using SurvivorsGame.Items.Projectiles;

namespace SurvivorsGame.Items.Offensive;

public partial class Whip : BaseOffensive
{
    [Export]
    private PackedScene _projectileScene;

    private BaseProjectile _projectileInstance;

    private double _t;

    public override void _Ready()
    {
        var projectile = _projectileScene.Instantiate<ProjectileWhip>();
        _projectileInstance = projectile;
        _t = Stats.AttackSpeed;
    }

    public override void _PhysicsProcess(double delta)
    {
        _t -= delta;
        if (_t <= 0)
        {
            Attack();
            _t = Stats.AttackSpeed;
        }
    }

    protected override void Attack()
    {
    }
}