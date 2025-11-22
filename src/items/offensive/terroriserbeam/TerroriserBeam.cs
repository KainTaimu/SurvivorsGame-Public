using SurvivorsGame.Items.Projectiles;
using SurvivorsGame.UI;

namespace SurvivorsGame.Items.Offensive;

// ProjectileSpeed is how fast the laser length grows
public partial class TerroriserBeam : BaseOffensive
{
    [Export]
    private PackedScene _projectileScene;

    private double _t;

    public override void _Ready()
    {
        _t = Stats.AttackSpeed * PlayerStats.AttackSpeedMultiplier;
    }

    public override void _PhysicsProcess(double delta)
    {
        _t -= delta;
        if (_t <= 0)
        {
            Attack();
            _t = Stats.AttackSpeed * PlayerStats.AttackSpeedMultiplier;
        }
    }

    protected override void Attack()
    {
        var projectile = _projectileScene.Instantiate<LaserBeam>();
        projectile.WeaponOrigin = this;
        projectile.SetScale(new Vector2(1, 1) * Stats.ProjectileScaleMultiplier);
        projectile.SetPosition(Player.Position);
        projectile.SetRotation(Crosshair.Instance.AngleFromPlayer);
        projectile.ProjectileSpeed = Stats.ProjectileSpeed;
        projectile.PierceLimit = Stats.PierceLimit;
        projectile.HitEnemy += HandleHit;
        AddChild(projectile);
    }
}
