using SurvivorsGame.Items.Projectiles;
using SurvivorsGame.UI;

namespace SurvivorsGame.Items.Offensive;

public partial class Pistol : BaseOffensive
{
    private double _t;

    [Export]
    protected PackedScene ProjectileScene;

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
        var projectile = ProjectileScene.Instantiate<BaseProjectile>();
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
