using Game.UI;

namespace Game.Levels.Controllers;

[GlobalClass]
public abstract partial class AbstractEnemyCollisionSolver : Node, IFrameTimeTrackable
{
	[Export]
	public NavMap? NavMap { get; private set; }

	[Export]
	public FrameTime FrameTime { get; private set; } = null!;
}
