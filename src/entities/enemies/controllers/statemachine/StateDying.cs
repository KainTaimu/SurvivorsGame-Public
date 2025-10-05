using SurvivorsGame.Pickups;
using SurvivorsGame.Systems;

namespace SurvivorsGame.Entities.Enemies.States;

public partial class StateDying : State
{
    [Export]
    private AnimatedSprite2D _animatedSprite;

    [Export]
    private BotStatController _botStatController;

    private float _dyingTime = 0.275f; // 0.275f

    private float _moveSpeed;

    private Vector2 _moveVector;

    [Export]
    private PackedScene _xpScene;

    public override string StateName { get; protected set; } = "dying";

    public override void Enter()
    {
        base.Enter();
        if (Entered)
        {
            return;
        }

        Entered = true;

        EnemyOwner.BotHitbox.SetDeferred("disabled", true);
        EnemyOwner.BotDamageBox.SetDeferred("disabled", true);
        // EnemyOwner.GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);

        _moveSpeed = _botStatController.MoveSpeed;
        _moveVector = EnemyOwner.Position.DirectionTo(Player.Position) * _moveSpeed;

        Vanish();

        if (_xpScene is not null)
        {
            SpawnXp();
        }
    }

    public override void PhysicsUpdate(double delta)
    {
        MoveChase(delta);
        Die(delta);
    }

    private void Die(double delta)
    {
        _dyingTime -= (float)delta;

        if (!(_dyingTime <= 0))
        {
            return;
        }

        GameWorld.Instance.Enemies.Remove(EnemyOwner);
        EnemyOwner.QueueFree();
    }

    private void Vanish()
    {
        var tween = CreateTween().SetTrans(Tween.TransitionType.Linear);

        var spriteShaderMaterial = _animatedSprite.Material as ShaderMaterial;

        if (spriteShaderMaterial is null)
        {
            return;
        }

        spriteShaderMaterial!.SetShaderParameter("flash_state", 1f);
        tween.Parallel().TweenMethod(
            Callable.From((float i) => { spriteShaderMaterial.SetShaderParameter("flash_state", i); }), 0f, 1f,
            _dyingTime);

        tween.Parallel().TweenMethod(
            Callable.From((Color i) => { spriteShaderMaterial.SetShaderParameter("color", i); }), new Color(1, 1, 1),
            new Color(255, 255, 255, 0), _dyingTime);
    }

    private void MoveChase(double delta)
    {
        EnemyOwner.Position += _moveVector * (float)delta;

        EnemyOwner.Sprite.FlipH = !(_moveVector.X > 0);
    }

    private void SpawnXp()
    {
        var xp = _xpScene.Instantiate<BaseXp>();
        xp.SetXp(_botStatController.XpGain);
        xp.Position = EnemyOwner.Position;

        GetTree().Root.AddChild(xp);
    }
}