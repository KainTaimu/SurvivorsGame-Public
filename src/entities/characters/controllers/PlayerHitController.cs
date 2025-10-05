using SurvivorsGame.Entities.Enemies;
using SurvivorsGame.Systems;

namespace SurvivorsGame.Entities.Characters;

public partial class PlayerHitController : Node
{
    [Export]
    private PackedScene _deathEffect;

    private double _lastIFrame;

    [Export]
    private Player _owner;

    private Tween _tween;

    public override void _Process(double delta)
    {
        if (_lastIFrame > 0)
        {
            _lastIFrame -= delta;
        }

        ProcessColliders();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventKey { Pressed: true } keyEvent)
        {
            return;
        }

#if DEBUG
        switch (keyEvent.Keycode)
        {
            case Key.F1:
                DebugKill();
                break;
        }
#endif
    }

    private void ProcessColliders()
    {
        foreach (var node in _owner.GetOverlappingAreas())
        {
            switch (node)
            {
                case BotDamageBox enemy:
                    _owner.EmitSignal(nameof(Player.PlayerDamaged), enemy.EnemyOwner.BotStatController.MaxDamage);
                    break;
            }
        }
    }

    private void OnDamage(int damage)
    {
        if (_lastIFrame > 0 || !_owner.Alive)
        {
            return;
        }

        // Always deal 5% DMG regardless of DEF
        var damageAfterDefense =
            Math.Clamp(damage - _owner.StatController.PlayerStats.Defense * 0.95, 0, double.PositiveInfinity) +
            damage * 0.05;
        var randomDamage = damageAfterDefense * GD.RandRange(-0.15f, 0.15f);
        var sumDamage = damageAfterDefense + randomDamage;
        var clampedDamage = Math.Clamp(sumDamage * _owner.StatController.PlayerStats.IncomingDamageMultiplier, 0,
            float.PositiveInfinity);
        var netDamage = Math.Ceiling(clampedDamage);

        _owner.StatController.PlayerStats.Health -= (int)netDamage;
        _lastIFrame = _owner.StatController.PlayerStats.InvincibilityTime;

        // Logger.LogDebug(
        // $"Player damaged {netDamage} ([{damage} - {damageAfterDefense}] + {Math.Round(Math.Abs(randomDamage))} *
        // {_owner.StatController.PlayerStats.IncomingDamageMultiplier})");

        DamagedFeedback();
        if (_owner.StatController.PlayerStats.Health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        _owner.StatController.PlayerStats.Health = 0;

        _owner.SetDeferred(Area2D.PropertyName.Monitorable, false);
        _owner.SetDeferred(Area2D.PropertyName.Monitoring, false);

        var scene = _deathEffect.Instantiate<CpuParticles2D>();
        scene.Position = _owner.Position;
        scene.Emitting = true;
        GetTree().CreateTimer(3, false).Timeout += () => { scene.QueueFree(); };
        AddChild(scene);

        _owner.Alive = false;
        _owner.StatController.PlayerStats.MoveSpeed = 0;
        GameWorld.Instance.EmitSignal(GameWorld.SignalName.PlayerDied);
        Logger.LogDebug("Player died");
        Vanish();
    }

    private void Vanish()
    {
        _tween?.Kill();
        _tween = CreateTween().SetTrans(Tween.TransitionType.Linear).SetParallel();

        if (_owner.Sprite.Material is not ShaderMaterial spriteShaderMaterial)
        {
            return;
        }

        spriteShaderMaterial.SetShaderParameter("color", new Color("red"));

        _tween.TweenMethod(Callable.From((float i) => { spriteShaderMaterial.SetShaderParameter("flash_state", i); }),
            0f, 1f, 1f);
        _tween.TweenMethod(Callable.From((Color c) => { spriteShaderMaterial.SetShaderParameter("color", c); }),
            new Color("red"), new Color(255, 0, 0, 0), 1f);
    }

    // Duplicate of Vanish method in StateDying
    private void DamagedFeedback()
    {
        var spriteShaderMaterial = _owner.Sprite.Material as ShaderMaterial;

        if (spriteShaderMaterial is null)
        {
            return;
        }

        _tween?.Kill();
        _tween = CreateTween().BindNode(this).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.Out);

        spriteShaderMaterial.SetShaderParameter("flash_state", 1f);
        spriteShaderMaterial.SetShaderParameter("color", new Color("white"));

        _tween.TweenMethod(Callable.From((float i) => { spriteShaderMaterial.SetShaderParameter("flash_state", i); }),
            1f, 0f, 1f);
    }

    private void DebugKill()
    {
        if (!_owner.Alive)
        {
            return;
        }

        Die();
        Logger.LogDebug("Emitted PlayerDied");
    }
}