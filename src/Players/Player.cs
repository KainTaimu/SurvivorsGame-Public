namespace Game.Players;

public partial class Player : Node2D
{
	[Export]
	public Character Character { get; private set; } = null!;

	public bool IsAlive => Character.CharacterStats.Health == 0;
}
