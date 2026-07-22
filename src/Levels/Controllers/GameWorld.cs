using Arch.Core;
using Game.Players;
using Schedulers;

namespace Game.Levels.Controllers;

[GlobalClass]
public partial class GameWorld : Node
{
	[Export]
	public Player MainPlayer { get; private set; } = null!;

	public static GameWorld Instance { get; private set; } = null!;

	public static World World { get; private set; } = null!;

	public override void _EnterTree()
	{
		World = World.Create(entityCapacity: 20_000);
		World.SharedJobScheduler = new JobScheduler(new JobScheduler.Config { ThreadPrefixName = "GameWorld" });
		Instance = this;
	}

	public override void _ExitTree()
	{
		World.SharedJobScheduler?.Dispose();
		World.Dispose();
		World = null!;
	}
}
