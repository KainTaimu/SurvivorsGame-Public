using Game.Core.ECS;
using Game.Core.Services;

namespace Game.Levels.Controllers;

public partial class EnemySpawner : Node
{
    [Export]
    private EntityComponentStore _entities = null!;

    private double _t;
    private int Spawned
    {
        get;
        set { field = Math.Clamp(value, 0, EntityComponentStore.MAX_SIZE); }
    }

    public override void _Process(double delta)
    {
        _t -= delta;
        SpawnEnemy();
    }

    public void SpawnEnemy()
    {
        if (_t > 0 || Spawned >= EntityComponentStore.MAX_SIZE)
            return;
        _t = 0.5f;

        var ss = ServiceLocator.GetService<SpriteFrameMappingsService>();
        if (ss is null)
            return;

        for (var i = 0; i < 500; i++)
        {
            var pos = new Vector2(GD.RandRange(1920, 1920 * 3), GD.RandRange(1920, 1920 * 3));
            if (!_entities.RegisterEntity(Spawned))
                continue;

            _entities.RegisterComponent(Spawned, new PositionComponent() { Position = pos });
            _entities.RegisterComponent(
                Spawned,
                new AnimatedSpriteComponent()
                {
                    SpriteName = "duck",
                    AnimationSpeed = 1,
                    FrameCount = 1,
                }
            );
            Spawned++;
        }
        Logger.LogDebug(Spawned);
    }
}
