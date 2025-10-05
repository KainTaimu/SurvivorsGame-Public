using SurvivorsGame.Entities.Enemies;

[GlobalClass]
public partial class BaseEffect : Resource
{
    protected bool Applied = false;

    [Export]
    public float EffectDuration;

    [Export]
    public float EffectValue;

    public BaseEnemy Target;

    public virtual string EffectName { get; protected set; }

    public override string ToString()
    {
        return $"{EffectName}({EffectValue}f, {Math.Round(EffectDuration, 2)}s)";
    }

    public virtual void Enter()
    {
    }

    public virtual void Exit()
    {
    }

    public virtual void Apply(float delta)
    {
        if (EffectDuration <= 0)
        {
            return;
        }

        EffectDuration -= delta;
    }

    public virtual void AddWorkTime(float delta)
    {
    }
}