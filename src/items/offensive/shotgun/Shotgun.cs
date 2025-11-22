using SurvivorsGame.Items.Projectiles;
using SurvivorsGame.UI;

namespace SurvivorsGame.Items.Offensive;

public partial class Shotgun : BaseOffensive
{
    private float _dispersionArc = Mathf.Pi / 16f;

    private int _pelletCount = 8;

    private float _pelletSpeedDeviation = 750f;

    [Export]
    private PackedScene _projectileScene;

    private double _t;

    public override void _Ready()
    {
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
        for (var i = 0; i < _pelletCount; i++)
        {
            var randomRotation = (float)GD.RandRange(-_dispersionArc, _dispersionArc);
            var randomPelletSpeed = (float)
                GD.RandRange(Stats.ProjectileSpeed - _pelletSpeedDeviation, Stats.ProjectileSpeed);

            var projectile = _projectileScene.Instantiate<ProjectileBullet>();
            projectile.WeaponOrigin = this;

            projectile.SetScale(new Vector2(1, 1) * Stats.ProjectileScaleMultiplier);
            projectile.SetPosition(Player.Position);
            projectile.SetRotation(Crosshair.Instance.AngleFromPlayer + randomRotation);

            projectile.ProjectileSpeed = randomPelletSpeed;
            projectile.PierceLimit = Stats.PierceLimit;
            projectile.HitEnemy += HandleHit;
            AddChild(projectile);
        }
    }
}
