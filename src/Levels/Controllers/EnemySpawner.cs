using Game.Core;
using Game.Core.ECS;
using Game.Core.Services;

namespace Game.Levels.Controllers;

public partial class EnemySpawner : Node
{
    [Export]
    private EntityComponentStore _entities = null!;

    public int TotalSpawned
    {
        get;
        private set
        {
            // Spawned should not be decremented because we rely on it to create unique ids
            field =
                Math.Clamp(value, field, EntityComponentStore.MAX_SIZE);
        }
    }

    public int Alive
    {
        get;
        private set
        {

            field = Math.Clamp(value, 0, EntityComponentStore.MAX_SIZE);
        }
    }

    private double _t;

    public override void _Ready()
    {
        _entities.BeforeEntityUnregistered += (_) => Alive--;
    }

    public override void _Process(double delta)
    {
        _t -= delta;
        SpawnEnemy();
    }

    public void SpawnEnemy()
    {
        if (_t > 0 || Alive >= EntityComponentStore.MAX_SIZE)
            return;
        _t = 0.05f;

        var ss = ServiceLocator.GetService<SpriteFrameMappingsService>();
        if (ss is null)
            return;

        for (var i = 0; i < 100; i++)
        {
            var pos = new Vector2(GD.RandRange(1920, 1920 * 3), GD.RandRange(1920, 1920 * 3));
            var id = TotalSpawned;
            if (!_entities.RegisterEntity(id))
                continue;

            // TODO: Enemy blueprints
            _entities.RegisterComponent(id, new HealthComponent(10));
            _entities.RegisterComponent(id, new EntityTypeComponent(EntityType.Enemy));
            _entities.RegisterComponent(id, new PositionComponent() { Position = pos });
            _entities.RegisterComponent(
                id,
                new AnimatedSpriteComponent()
                {
                    SpriteName = "duck",
                    AnimationSpeed = 1,
                    FrameCount = 1,
                }
            );
            TotalSpawned++;
            Alive++;
        }
        Logger.LogDebug(TotalSpawned);
    }
}
