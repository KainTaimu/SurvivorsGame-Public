using SurvivorsGame.Entities.Characters;
using SurvivorsGame.Systems;

namespace SurvivorsGame.Items.Effects;

public partial class EffectKnockback : BaseEffect
{
    private Vector2 _finalVector;

    private Vector2 _pushVector;

    private Tween _tween;

    public override string EffectName { get; protected set; } = "knockback";
    private Player Player => GameWorld.Instance.MainPlayer;

    public override void Enter()
    {
        if (Applied)
        {
            return;
        }

        _pushVector = new Vector2(EffectValue, 0).Rotated(
            Target.Position.AngleToPoint(Player.Position)
        );
        _finalVector = new Vector2(
            Target.Position.X - _pushVector.X,
            Target.Position.Y - _pushVector.Y
        );
        Target.Position = _finalVector;

        Applied = true;
    }
}
