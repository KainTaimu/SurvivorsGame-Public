using System.Collections.Generic;

namespace Game.Items.Offensive;

public partial class M870 : Firearm
{
    [Export]
    private AudioStreamPlayer? _shellReloadAudioPlayer;

    [Export]
    private AudioStreamPlayer? _cockingAudioPlayer;

    private bool _isShotgunReloading;
    private double _reloadCooldown;

    private int PelletCount =>
        Stats.Additional.GetValueOrDefault("PelletCount").AsInt32();

    public override void _Ready()
    {
        base._Ready();
        OnAttack += ApplyCameraRecoil;
    }

    public override void Attack()
    {
        if (MagazineCount <= 0)
        {
            Reload();
            return;
        }

        if (!IsReadyToShoot)
            return;
        _isShotgunReloading = false;

        ShootAudioPlayer?.Play();
        if (_cockingAudioPlayer is not null)
        {
            GetTree().CreateTimer(Stats.AttackSpeed / 2).Timeout += () =>
                _cockingAudioPlayer.Play();
        }

        FireCooldown = Stats.AttackSpeed;
        MagazineCount--;

        var playerVector = Player.GetCanvasTransform() * Player.Position;

        Vector2 mouseVector;
        if (Crosshair is not null)
        {
            mouseVector =
                Crosshair.PrimaryCrosshairSprite.GetCanvasTransform()
                * Crosshair.PrimaryCrosshairSprite.GlobalPosition;
        }
        else
            mouseVector = Player.GetGlobalMousePosition();

        var baseRotation = playerVector.AngleToPoint(mouseVector);
        for (var i = 0; i < PelletCount; i++)
        {
            var bloomRad = BloomCoefficientDeg * (Math.PI / 180);
            var bloom = (float)GD.RandRange(-bloomRad / 2, bloomRad / 2);

            var rotation = baseRotation + bloom;

            var projectile = ProjectilePool.GetProjectile();

            projectile.Origin = this;
            projectile.SetScale(Vector2.One * Stats.ProjectileScaleMultiplier);
            projectile.SetPosition(Player.Position);
            projectile.SetRotation(rotation);
            projectile.ProjectileSpeed =
                Stats.ProjectileSpeed * (float)GD.RandRange(1f, 2f);
            projectile.PierceLimit = Stats.PierceLimit;
            projectile.HitRadius = FirearmStats?.ProjectileRadius ?? 24;
            projectile.Initialize();
        }

        ApplyCursorRecoil();
        EmitSignalOnAttack();
    }

    private Tween? _reloadTween;

    public override void Reload()
    {
        if (_reloadCooldown > 0)
            return;
        if (MagazineCount == MagazineCapacity)
            return;

        _reloadTween?.Kill();
        _reloadTween = null;
        _isShotgunReloading = true;

        // Add delay before reloading sequence to punish reload/shoot spam

        _reloadTween = CreateTween().SetLoops(MagazineCapacity - MagazineCount);
        _reloadTween
            .TweenCallback(
                Callable.From(() =>
                {
                    if (!_isShotgunReloading)
                        return;
                    MagazineCount++;
                    _shellReloadAudioPlayer?.Play();
                    if (MagazineCount == MagazineCapacity)
                    {
                        _isShotgunReloading = false;
                        _reloadTween.Kill();
                    }
                })
            )
            .SetDelay(ReloadTime);
    }

    public override void _Process(double delta)
    {
        FireCooldown -= delta;

        if (Input.IsActionJustPressed(InputMapNames.WeaponReload))
        {
            Reload();
            return;
        }

        if (
            !Input.IsActionJustPressed(
                AttackActionString ?? InputMapNames.PrimaryAttack
            )
        )
            return;

        Attack();
    }
}
