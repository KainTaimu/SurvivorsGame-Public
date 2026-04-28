using Game.Core.ECS;

namespace Game.Levels.Controllers;

public partial class EnemyMover : Node
{
	[Export]
	private EntityComponentStore _entities = null!;

	public override void _Process(double delta)
	{
		var player = GameWorld.Instance.MainPlayer;
		if (player is null)
			return;
		var playerPos = player.GlobalPosition;

		foreach (
			var (id, pos, moveSpeed) in _entities.Query<
				PositionComponent,
				MoveSpeedComponent
			>()
		)
		{
			var p = pos.Position.MoveToward(
				playerPos,
				moveSpeed.MoveSpeed * (float)delta
			);

			_entities.UpdateComponent(id, pos with { Position = p });
		}
	}
}
