using Game.Levels;
using Game.Levels.Controllers;

namespace Game.UI;

public partial class PerformanceMonitor : CanvasLayer
{
	[Export]
	public EnemyCollisionSolver? CollisionSolver;

	[Export]
	public EnemyRenderer? EnemyRenderer;

	[Export]
	public EnemyNavMeshMover? Nav;

	[Export]
	private BoxContainer _boxContainer = null!;

	[Export]
	private RichTextLabel _collisionSolverlabel = null!;

	[Export]
	private RichTextLabel _rendererlabel = null!;

	[Export]
	private RichTextLabel _navLabel = null!;

	public override void _Process(double delta)
	{
		if (CollisionSolver is not null)
		{
			_collisionSolverlabel.Text =
				CollisionSolver.ProcessTime < 10
					? $"Collisions: {CollisionSolver.ProcessTime}ms"
					: $"[color=orange]Collisions: {CollisionSolver.ProcessTime}ms[/color]";
		}

		if (EnemyRenderer is not null)
		{
			_rendererlabel.Text =
				EnemyRenderer.ProcessTime < 5
					? $"Rendering: {EnemyRenderer.ProcessTime}ms"
					: $"[color=orange]Rendering: {EnemyRenderer.ProcessTime}ms[/color]";
		}

		if (Nav is not null)
		{
			_navLabel.Text =
				Nav.ProcessTime < 10 ? $"Nav: {Nav.ProcessTime}ms" : $"[color=orange]Nav: {Nav.ProcessTime}ms[/color]";
		}
	}
}
