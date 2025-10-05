using SurvivorsGame.Entities.Enemies;

[GlobalClass]
public partial class BaseEffect : Resource
{
    public virtual string EffectName { get; protected set; }
    [Export] public float EffectValue;
    [Export] public float EffectDuration;

    public BaseEnemy Target;
    protected bool Applied = false;

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