using SurvivorsGame.Entities.Enemies;
using SurvivorsGame.Items.Projectiles;

namespace SurvivorsGame.Items.Offensive;

public partial class Pistol : BaseOffensive
{
    [Export] protected PackedScene ProjectileScene;
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
        var projectile = ProjectileScene.Instantiate<BaseProjectile>();
        projectile.WeaponOrigin = this;
        projectile.SetScale(new Vector2(1, 1) * Stats.ProjectileScaleMultiplier);
        projectile.SetPosition(Player.Position);
        projectile.SetRotation(Player.PlayerMovementController.Facing);
        projectile.ProjectileSpeed = Stats.ProjectileSpeed;
        projectile.PierceLimit = Stats.PierceLimit;
        projectile.HitEnemy += HandleHit;
        AddChild(projectile);
    }

    private void HandleHit(BaseEnemy target)
    {
        var damageEffect = new EffectDamage
        {
            EffectValue = Stats.Damage + CalculateCrit(),
            EffectDuration = 0f
        };

        target.EmitSignal(nameof(BaseEnemy.EnemyHit), damageEffect);

        foreach (var effect in Stats.ProjectileEffects)
        {
            target.EmitSignal(nameof(BaseEnemy.EnemyHit), effect.Duplicate(true));
        }
    }

    private float CalculateCrit()
    {
        var roll = GD.Randf();
        if (roll > Stats.CritChanceProportion)
        {
            return 0f;
        }

        return Stats.Damage * Stats.CritDamageMultiplier;
    }
}