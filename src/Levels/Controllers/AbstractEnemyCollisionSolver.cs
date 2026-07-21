using Game.UI;

namespace Game.Levels.Controllers;

public partial class AbstractEnemyCollisionSolver : Node, IFrameTimeTrackable
{
	[Export]
	public FrameTime FrameTime { get; private set; } = null!;
}
