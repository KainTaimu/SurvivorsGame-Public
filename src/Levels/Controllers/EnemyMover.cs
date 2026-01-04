using Game.Core.ECS;

namespace Game.Levels.Controllers;

public partial class EnemyMover : Node
{
    [Export]
    private EntityComponentStore _entities = null!;

    public override void _Process(double delta)
    {
        var player = GameWorld.Instance.MainPlayer;
        if (player is null)
            return;
        var playerPos = player.GlobalPosition;

        foreach (var (id, pos) in _entities.Query<PositionComponent>())
        {
            var p = pos.Position.MoveToward(playerPos, 150 * (float)delta);

            _entities.UpdateComponent(id, new PositionComponent() { Position = p });
        }
    }
}
