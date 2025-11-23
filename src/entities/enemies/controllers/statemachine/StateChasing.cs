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

        var direction = PlayerVectorFieldGrid.Instance.GetDirectionTo(enemyPosition);

        // HACK: Because Godot Vector2 is not nullable
        if (direction == Vector2.Inf)
        {
            direction = enemyPosition.DirectionTo(playerPosition) * _botStatController.MoveSpeed;
            Logger.LogWarning("Got infinity vector");
        }

        var moveVector = direction * _botStatController.MoveSpeed;

        EnemyOwner.Position += moveVector * (float)delta;

        EnemyOwner.Sprite.FlipH = !(moveVector.X > 0);
    }
}
