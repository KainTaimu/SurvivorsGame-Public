using Game.Players;

namespace Game.Levels.Controllers;

public partial class GameWorld : Node
{
    [Export]
    public Player? MainPlayer { get; private set; }

    public static GameWorld Instance { get; private set; } = null!;

    public override void _EnterTree()
    {
        Instance = this;
    }
}
