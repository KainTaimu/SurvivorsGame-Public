using Game.Core;

namespace Game.Players;

public partial class Player : Node2D
{
    [Export]
    public Character Character { get; private set; } = null!;

    public EntityType EntityType => EntityType.Player;
}
