using Game.Players;

namespace Game.Levels.Controllers;

public partial class GameWorldInstance : Node
{
	public Player? MainPlayer;
	public LevelData? LevelData;

	public static GameWorldInstance Instance = null!;

	public override void _EnterTree()
	{
		Instance = this;
	}
}
