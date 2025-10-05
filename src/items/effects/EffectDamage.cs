public partial class EffectDamage : BaseEffect
{
    private bool _applied;

    public override string EffectName { get; protected set; } = "damage";

    public override void Exit()
    {
    }

    public override void Apply(float delta)
    {
        base.Apply(delta);

        Target.BotStatController.Health -= EffectValue;

        if (!_applied)
        {
            Target.BotStatController.Damage((int)Math.Ceiling(EffectValue));
        }

        _applied = true;
    }
}