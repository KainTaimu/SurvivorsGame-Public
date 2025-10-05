using SurvivorsGame.Entities.Characters;
using SurvivorsGame.Systems;

namespace SurvivorsGame.Entities.Enemies.States;

public partial class State : Node
{
    [Signal]
    public delegate void TransitionedEventHandler(string toState);

    protected BaseEnemy EnemyOwner;

    protected bool Entered;

    public virtual string StateName { get; protected set; }
    protected static Player Player => GameWorld.Instance.MainPlayer;

    public virtual void Enter()
    {
        EnemyOwner = GetParent<StateMachine>().EnemyOwner;
    }

    public virtual void Exit()
    {
    }

    public virtual void Update(double delta)
    {
    }

    public virtual void PhysicsUpdate(double delta)
    {
    }
}