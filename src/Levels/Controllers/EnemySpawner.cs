using Game.Core.ECS;
using Game.Core.Services;

namespace Game.Levels.Controllers;

public partial class EnemySpawner : Node
{
    [Export]
    private EntityComponentStore _entities = null!;

    public override void _Ready()
    {
        var ss = ServiceLocator.GetService<SpriteFrameMappingsService>();
        if (ss is null)
            return;

#if DEBUG
        for (var i = 0; i < 15_000; i++)
#else
        for (var i = 0; i < 25_000; i++)
#endif
        {
            var pos = new Vector2(GD.RandRange(1920, 1920 * 3), GD.RandRange(1920, 1920 * 3));
            if (!_entities.RegisterEntity(i))
                continue;

            _entities.RegisterComponent(i, new PositionComponent() { Position = pos });
            _entities.RegisterComponent(
                i,
                new AnimatedSpriteComponent()
                {
                    SpriteName = "duck",
                    AnimationSpeed = 1,
                    FrameCount = 1,
                }
            );
        }
    }

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

        // _entities
        //     .Query<PositionComponent>()
        //     .AsParallel()
        //     .ForAll(
        //         (x) =>
        //         {
        //             var (id, pos) = x;
        //             var p = pos.Position.MoveToward(playerPos, 150 * (float)delta);
        //             _entities.UpdateComponent(id, new PositionComponent() { Position = p });
        //         }
        //     );
    }
}
