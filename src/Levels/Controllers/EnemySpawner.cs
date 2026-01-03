using Game.Core.ECS;
using Game.Core.Services;
using Game.Utils;

namespace Game.Levels.Controllers;

public partial class EnemySpawner : Node
{
    [Export]
    private EntityComponentStore _entities = null!;

    private DebugRectCreator Db => new(GetTree());

    public override void _Ready()
    {
        var ss = ServiceLocator.GetService<SpriteFrameMappingsService>();
        if (ss is null)
            return;

        for (var i = 0; i < 10_000; i++)
        {
            var pos = new Vector2(GD.RandRange(0, 1920), GD.RandRange(0, 1080));
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
            // Callable.From(() => Db.CreateRect(pos, Vector2.One * 50)).CallDeferred();
        }
    }

    public override void _Process(double delta)
    {
        var player = GameWorld.Instance.MainPlayer;
        if (player is null)
            return;

        foreach (var (id, pos) in _entities.Query<PositionComponent>())
        {
            var p = pos.Position.MoveToward(player.GlobalPosition, 500 * (float)delta);
            _entities.UpdateComponent(id, new PositionComponent() { Position = p });
        }
    }
}
