using System.Linq;
using Game.Core;
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

        var enemies = _entities
            .Query<PositionComponent, EntityTypeComponent>()
            .Where(x => x.Item3.EntityType == EntityType.Enemy)
            .Select(x => new { x.Item1, x.Item2 });

        foreach (var x in enemies)
        {
            var (id, pos) = (x.Item1, x.Item2);

            var p = pos.Position.MoveToward(playerPos, 150 * (float)delta);

            _entities.UpdateComponent(id, new PositionComponent() { Position = p });
        }
    }
}
