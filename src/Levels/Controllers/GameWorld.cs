using Game.Players;

namespace Game.Levels.Controllers;

public partial class GameWorld : Node
{
	[Export]
	public Player MainPlayer { get; private set; } = null!;

	[Export]
	private TileMapLayer _levelTileMap = null!;

	public Vector2 LevelDimensions =>
		_levelTileMap.GetUsedRect().Size * _levelTileMap.TileSet.TileSize;

	public static GameWorld Instance { get; private set; } = null!;

	public override void _EnterTree()
	{
		Instance = this;
	}
}
