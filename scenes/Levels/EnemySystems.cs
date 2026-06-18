namespace Game.Levels.Controllers;

public partial class EnemySystems : Node
{
	public static EnemySystems Instance = null!;

	public override void _EnterTree()
	{
		Instance = this;
	}
}
