namespace SurvivorsGame.Entities.Enemies.States;

public partial class StateIdle : State
{
    public override string StateName { get; protected set; } = "idle";

    [Export] private BaseEnemy _owner;

    public override void PhysicsUpdate(double delta)
    {
    }
}