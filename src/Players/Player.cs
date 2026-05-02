using Game.Players.Controllers;

namespace Game.Players;

public partial class Player : Node2D
{
	[Export]
	public Character Character { get; private set; } = null!;

	[Export]
	public PlayerMovementController MovementController { get; private set; } =
		null!;

	public bool IsAlive => Character.CharacterStats.Health == 0;
}
