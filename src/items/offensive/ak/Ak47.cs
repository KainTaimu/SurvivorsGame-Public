using System.Threading.Tasks;
using SurvivorsGame.Entities.Enemies;
using SurvivorsGame.Items.Projectiles;

namespace SurvivorsGame.Items.Offensive;

public partial class Ak47 : BaseOffensive
{
    [Export]
    private PackedScene _projectileScene;

    private double _fireCooldown;
    private bool _isReloading;

    private int _reloadTimeMs = 1500;
    private float _bloomCoefficient = 0.03f;
    private int _magazineCapacity = 30;
    private int _magazineCount;

    public override void _Ready()
    {
        _magazineCapacity = (int)Stats.Additional["MagazineCapacity"];
        _magazineCount = _magazineCapacity;
        _reloadTimeMs = (int)Stats.Additional["ReloadTimeMs"];
        _bloomCoefficient = (float)Stats.Additional["BloomCoefficient"];
    }

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
}
