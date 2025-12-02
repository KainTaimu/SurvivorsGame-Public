using SurvivorsGame.Levels.Systems;

namespace SurvivorsGame.Entities.Enemies.States;

public partial class StateChasing : State
{
    public override string StateName { get; protected set; } = "chasing";

    public override void PhysicsUpdate(double delta)
    {
        MoveChase(delta);
    }

    private void MoveChase(double delta)
    {
        var enemyPosition = EnemyOwner.Position;
        var playerPosition = GlobalStateController.Instance.CachedPlayerState.Position;

        var fieldGrid = PlayerVectorFieldGrid.Instance;
        Vector2 direction;
        if (fieldGrid is null)
        {
            direction = enemyPosition.DirectionTo(playerPosition);
        }
        else
        {
            direction = fieldGrid.GetDirectionTo(enemyPosition);

            // HACK: Because Godot Vector2 is not nullable
            if (direction == Vector2.Inf)
            {
                direction = enemyPosition.DirectionTo(playerPosition);
                Logger.LogWarning("Got infinity vector");
            }
        }

        var moveVector = direction * EnemyOwner.Stats.MoveSpeed;

        EnemyOwner.Position += moveVector * (float)delta;

        // BUG: REGRESSION: Sprite flipping has not been implemented
        // EnemyOwner.Sprite.FlipH = !(moveVector.X > 0);
    }
}
