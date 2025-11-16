using SurvivorsGame.VFX;

namespace SurvivorsGame.Items.Effects;

public partial class EffectCritDamage : BaseEffect
{
    private bool _applied;

    public override string EffectName { get; protected set; } = "CritDamage";

    public override void Exit() { }

    public override void Apply(float delta)
    {
        base.Apply(delta);

        Target.BotStatController.Health -= EffectValue;

        if (_applied)
            return;

        // Target.BotStatController.Damage((int)Math.Ceiling(EffectValue));
        var indicatorScene = GD.Load<PackedScene>("uid://cdvtgev1dig3f");
        var damageIndicator = indicatorScene.Instantiate<DamageIndicator>();

        Target.GetTree().Root.AddChild(damageIndicator);
        damageIndicator.ShowIndicator(Target, (int)Math.Ceiling(EffectValue), true);

        _applied = true;
    }
}
