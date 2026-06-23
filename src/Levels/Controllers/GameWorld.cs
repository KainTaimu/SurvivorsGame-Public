using Arch.Core;
using Game.Players;

namespace Game.Levels.Controllers;

public partial class GameWorld : Node
{
	[Export]
	public Player MainPlayer { get; private set; } = null!;

	public static GameWorld Instance { get; private set; } = null!;

	public static World World { get; private set; } = null!;

	public override void _EnterTree()
	{
		World = World.Create();
		Instance = this;
	}

	public override void _ExitTree()
	{
		World.Dispose();
		World = null!;
	}
}
