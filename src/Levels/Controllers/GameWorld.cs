using Game.Players;

namespace Game.Levels.Controllers;

public partial class GameWorld : Node
{
    [Export]
    public Player? MainPlayer { get; private set; }

    [Export]
    private TileMapLayer LevelTileMap = null!;

    public Vector2 LevelDimensions
    {
        get => LevelTileMap.GetUsedRect().Size * LevelTileMap.TileSet.TileSize;
    }

    public static GameWorld Instance { get; private set; } = null!;

    public override void _EnterTree()
    {
        Instance = this;
    }
}
