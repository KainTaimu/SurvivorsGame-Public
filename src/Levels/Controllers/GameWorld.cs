using Game.Players;

namespace Game.Levels;

public partial class GameWorld : Node
{
    [Export]
    public Player? MainPlayer { get; private set; }
}
