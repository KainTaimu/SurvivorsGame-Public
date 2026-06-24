using Game.Levels.Controllers;

namespace Game.UI;

public partial class PerformanceMonitor : CanvasLayer
{
	[Export]
	public EnemyCollisionSolver? CollisionSolver;

	[Export]
	public EnemyRenderer? EnemyRenderer;

	[Export]
	private BoxContainer _boxContainer = null!;

	[Export]
	private Label _collisionSolverlabel = null!;

	[Export]
	private Label _rendererlabel = null!;

	public override void _Process(double delta)
	{
		_collisionSolverlabel.Text = $"Collisions: {CollisionSolver?.ProcessTime}ms";
		_rendererlabel.Text = $"Rendering: {EnemyRenderer?.ProcessTime}ms";
	}
}
