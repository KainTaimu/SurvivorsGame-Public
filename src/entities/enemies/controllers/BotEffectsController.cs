using System.Collections.Generic;
using SurvivorsGame.Items.Effects;

namespace SurvivorsGame.Entities.Enemies;

public partial class BotEffectsController : Node
{
    [Export]
    private BaseEnemy _owner;

    public List<BaseEffect> CurrentEffects { get; } = [];

    public override void _Process(double delta)
    {
        ProcessEffects(delta);
    }

    private void AddEffect(BaseEffect effect)
    {
        if (!IsInstanceValid(_owner))
        {
            return;
        }

        if (_owner.StateMachine.CurrentState.Name == "Dying")
        {
            return;
        }

        var x = CurrentEffects.Find(x =>
            x.EffectName == effect.EffectName
            && !(Math.Abs(x.EffectValue - effect.EffectValue) < 0.01f)
        );

        if (x is not null)
        {
            x.AddWorkTime(effect.EffectDuration);
            return;
        }

        effect.Target = _owner;
        CurrentEffects.Add(effect);
    }

    private void ProcessEffects(double delta)
    {
        for (var i = 0; i < CurrentEffects.Count; i++)
        {
            var effect = CurrentEffects[i];
            effect.Enter();
            effect.Apply((float)delta);

            if (!(effect.EffectDuration <= 0))
            {
                continue;
            }

            CurrentEffects[i].Exit();
            CurrentEffects.RemoveAt(i);
        }
    }
}

