using SurvivorsGame.Entities.Enemies;

public partial class EffectSlow : BaseEffect
{
    private BotStatController _botStatController;

    private float _initialDuration;

    private float _initialSpeed;

    public override string EffectName { get; protected set; } = "slow";

    public override void Enter()
    {
        if (Applied)
        {
            return;
        }

        _botStatController = Target.BotStatController;

        _initialDuration = EffectDuration;
        _initialSpeed = _botStatController.MaxMoveSpeed;
        _botStatController.MoveSpeed -= EffectValue;

        Applied = true;
    }

    public override void Exit()
    {
        _botStatController.MoveSpeed = _initialSpeed;
    }

    public override void Apply(float delta)
    {
        base.Apply(delta);

        if (EffectDuration < 0.3f * _initialDuration)
        {
            var clamped = EffectDuration / (0.5f * _initialDuration);
            Target.BotStatController.MoveSpeed = Mathf.Lerp(_initialSpeed, _initialSpeed - EffectValue, clamped);
        }
    }

    public override void AddWorkTime(float delta)
    {
        var result = MathF.Log10(delta);
        EffectDuration += result;
    }
}