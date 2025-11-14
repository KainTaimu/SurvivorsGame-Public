using System.Collections.Generic;
using SurvivorsGame.Items.Projectiles;

namespace SurvivorsGame.Items.Offensive;

public partial class Ak47 : BaseOffensive
{
    [Export]
    private PackedScene _projectileScene;

    private Queue<ProjectileBullet> _bulletPool = [];
    private Queue<ProjectileBullet> _usedBulletPool = [];
    private double _fireCooldown;

    public override void _Ready()
    {
        _fireCooldown = Stats.AttackSpeed;
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event is InputEventKey eventKey)
        {
            if (eventKey.Pressed && eventKey.Keycode == Key.R && _fireCooldown <= 0)
                Attack();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_fireCooldown <= 0)
            return;
        _fireCooldown -= delta;
    }

    protected override void Attack() { }

    private void PopulateBulletPool() { }
}

