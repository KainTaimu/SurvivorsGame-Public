using SurvivorsGame.Levels.Systems;

namespace SurvivorsGame.Entities.Enemies.States;

public partial class StateChasing : State
{
    [Export]
    private BotStatController _botStatController;

    public override string StateName { get; protected set; } = "chasing";

    public override void PhysicsUpdate(double delta)
    {
        MoveChase(delta);
    }

    private void MoveChase(double delta)
    {
        var enemyPosition = EnemyOwner.Position;
        var playerPosition = GlobalStateController.Instance.CachedPlayerState.Position;
        var moveVector = enemyPosition.DirectionTo(playerPosition) * _botStatController.MoveSpeed;

        EnemyOwner.Position += moveVector * (float)delta;

        EnemyOwner.Sprite.FlipH = !(moveVector.X > 0);
    }
}

