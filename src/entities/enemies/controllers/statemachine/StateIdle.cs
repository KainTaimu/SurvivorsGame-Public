namespace SurvivorsGame.Entities.Enemies.States;

public partial class StateIdle : State
{
    [Export]
    private BaseEnemy _owner;

    public override string StateName { get; protected set; } = "idle";

    public override void PhysicsUpdate(double delta)
    {
    }
}