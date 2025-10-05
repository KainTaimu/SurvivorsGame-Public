using SurvivorsGame.Levels.Systems;
using SurvivorsGame.Systems;

namespace SurvivorsGame.Levels;

public partial class BaseMap : TileMapLayer
{
    [Export]
    public WaveController WaveController { get; private set; }

    public Vector2 PixelSize => GetUsedRect().Size * TileSet.TileSize;

    public override void _EnterTree()
    {
        GameWorld.Instance.SetCurrentLevel(this);
    }
}