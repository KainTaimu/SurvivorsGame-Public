using System.Threading.Tasks;
using SurvivorsGame.Entities.Enemies;
using SurvivorsGame.Items.Projectiles;

namespace SurvivorsGame.Items.Offensive;

public partial class Ak47 : BaseOffensive
{
    [Export]
    private PackedScene _projectileScene;

    private double _fireCooldown;
    private int _magazineCount = 30;
    private const int _magazineCapacity = 30;
    private bool _isReloading;

    private const int _reloadTimeMs = 1500;
    private const float _bloomCoefficient = 0.03f;

    public override void _Ready() { }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventKey eventKey)
            return;

        if (eventKey.Keycode == Key.R && !_isReloading)
            Reload();
    }

    public override void _Process(double delta)
    {
        if (Input.IsMouseButtonPressed(MouseButton.Left))
        {
            if (_isReloading)
                return;

            if (_magazineCount <= 0)
            {
                Reload();
                return;
            }

            if (_fireCooldown > 0)
                return;

            Attack();
            _fireCooldown = Stats.AttackSpeed;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_fireCooldown <= 0)
            return;
        _fireCooldown -= delta;
    }

    protected override void Attack()
    {
        if (_magazineCount <= 0 || _isReloading)
            return;

        _magazineCount--;

        var bloom = (float)GD.Randfn(Player.PlayerMovementController.Facing, _bloomCoefficient);

        var projectile = _projectileScene.Instantiate<ProjectileBullet>();
        projectile.WeaponOrigin = this;
        projectile.SetScale(Vector2.One * Stats.ProjectileScaleMultiplier);
        projectile.SetPosition(Player.Position);
        projectile.SetRotation(bloom);
        projectile.ProjectileSpeed = Stats.ProjectileSpeed;
        projectile.PierceLimit = Stats.PierceLimit;
        projectile.HitEnemy += HandleHit;
        AddChild(projectile);
    }

    protected void Reload()
    {
        if (_magazineCount == _magazineCapacity)
            return;

        _isReloading = true;
        _ = ReloadTask();
    }

    private async Task ReloadTask()
    {
        await Task.Delay(_reloadTimeMs);
        _isReloading = false;
        _magazineCount = _magazineCapacity;
        Logger.LogDebug("done");
    }

    private void HandleHit(BaseEnemy target)
    {
        var damageEffect = new EffectDamage
        {
            EffectValue = Stats.Damage + CalculateCrit(),
            EffectDuration = 0f,
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
