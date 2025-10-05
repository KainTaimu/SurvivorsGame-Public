using System.Diagnostics.CodeAnalysis;
using SurvivorsGame.Entities.Enemies.States;

namespace SurvivorsGame.Entities.Enemies;

public partial class BotHitController : Node
{
    [Export]
    private AnimatedSprite2D _animatedSprite;

    [Export]
    private BotStatController _botStatController;

    private float _dyingTime = 0.19f;

    [Export]
    private BaseEnemy _owner;

    private ShaderMaterial _spriteShaderMaterial;

    [Export]
    private StateMachine _stateMachine;

    private Tween _tween;

    public override void _Ready()
    {
        if (_owner is null)
        {
            Logger.LogError($"{GetParent().Name} does not have an assigned HitController!");
        }

        if (_botStatController is null)
        {
            Logger.LogError($"{GetParent().Name} does not have an assigned BotStatController!");
        }

        if (_animatedSprite is null)
        {
            Logger.LogError($"{GetParent().Name} does not have an assigned AnimatedSprite!");
        }

        if (_stateMachine is null)
        {
            Logger.LogError($"{GetParent().Name} does not have an assigned StateMachine!");
        }

        _spriteShaderMaterial = _animatedSprite?.Material as ShaderMaterial;
    }

    public override void _Process(double delta)
    {
        if (_botStatController.Health <= 0)
        {
            _stateMachine.OnChildTransition("Dying");
        }
    }

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private void OnEnemyHit(BaseEffect effect)
    {
        if (_owner is null || _spriteShaderMaterial is null || _stateMachine.CurrentState is StateDying)
        {
            return;
        }

        _tween?.Kill();
        _tween = CreateTween().BindNode(this).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.Out);
        _spriteShaderMaterial.SetShaderParameter("flash_state", 1f);
        _spriteShaderMaterial.SetShaderParameter("color", new Color("white"));
        _tween.TweenMethod(Callable.From((float i) => { _spriteShaderMaterial.SetShaderParameter("flash_state", i); }),
            1f, 0f, 1f);
    }
}