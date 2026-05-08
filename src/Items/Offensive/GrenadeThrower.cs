using Game.Levels.Controllers;
using Game.UI;

namespace Game.Items.Offensive;

public partial class GrenadeThrower : BaseOffensive, IManualAttack
{
    [Export]
    public PackedScene GrenadeScene = null!;

    [Export]
    public float ThrowForce = 100;

    public string? AttackActionString { get; set; }

    private Crosshair? Crosshair => Crosshair.Instance;
    private double _fireCooldown;

    private ProjectilePool _projectilePool = null!;

    public override void _Ready()
    {
        _projectilePool = new ProjectilePool { ProjectileScene = GrenadeScene };
        AddChild(_projectilePool);
    }

    public override void _Process(double delta)
    {
        _fireCooldown -= delta;

        if (
            !Input.IsActionPressed(
                AttackActionString ?? InputMapNames.PrimaryAttack
            )
        )
            return;
        Attack();
    }

    public override void Attack()
    {
        if (Crosshair is null)
        {
            Logger.LogError("No crosshair");
            return;
        }

        if (_fireCooldown > 0)
            return;
        _fireCooldown = Stats.AttackSpeed;

        var nade = GrenadeScene.Instantiate<Grenade>();

        nade.OffensiveOrigin = this;
        nade.GlobalPosition = Player.GlobalPosition;
        var force =
            new Vector2(
                Mathf.Cos(Crosshair.AngleFromPlayer),
                Mathf.Sin(Crosshair.AngleFromPlayer)
            ) * ThrowForce;
        nade.ApplyImpulse(force);
        // GetParent().AddChild(nade);
        GetTree().Root.CallDeferred(Window.MethodName.AddChild, nade);
    }
}
