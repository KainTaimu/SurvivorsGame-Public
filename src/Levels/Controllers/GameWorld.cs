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

	public const int MAX_ECS_ENTITIES = 16_384; // 2^14

	public override void _EnterTree()
	{
		World = World.Create(entityCapacity: MAX_ECS_ENTITIES);
		World.SharedJobScheduler = new JobScheduler(new JobScheduler.Config { ThreadPrefixName = "GameWorld" });
		Instance = this;
	}

	public override void _ExitTree()
	{
		World.SharedJobScheduler?.Dispose();
		World.Dispose();
		World = null!;
	}

	public override void _Ready()
	{
		GameWorldInstance.Instance.MainPlayer = MainPlayer;
	}
}
