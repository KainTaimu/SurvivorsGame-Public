using Game.Core.ECS;
using Game.Models;

namespace Game.Levels.Controllers;

public partial class EnemyHitManager : Node
{
	[Export]
	private EntityComponentStore _entities = null!;

	private CenteredMovingUniformGrid<int> _grid = null!;

	public override void _EnterTree()
	{
		var viewport = GetViewport();
		if (viewport is null)
		{
			Logger.LogError("EnemyCollisionSolver: missing viewport.");
			return;
		}

		var windowSize = viewport.GetVisibleRect().Size * 3.0f;
		_grid = new CenteredMovingUniformGrid<int>(16, windowSize);

		_entities.BeforeEntityUnregistered += OnEnemyRemoved;
	}

	public override void _Process(double delta)
	{
		var player = GameWorld.Instance.MainPlayer;
		if (player is null)
			return;
		var playerPos = player.GlobalPosition;

		_grid.Recenter(playerPos);
	}

	private void OnEnemyRemoved(int id) { }
}
