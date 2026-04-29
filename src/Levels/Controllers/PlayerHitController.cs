using Game.Core.ECS;

namespace Game.Levels.Controllers;

public partial class PlayerHitController : Node
{
	[Export]
	private EntityComponentStore _componentStore = null!;

	[Export]
	private EnemyTargetQuery _hitManager = null!;

	public override void _Process(double delta) { }
}
