using Game.Core.ECS;
using Game.Levels.Controllers;

namespace Game.Items.Offensive;

public sealed partial class MarksmanRifle : Sniper
{
	private float NearbyPushRadius => FirearmStats.Additional["NearbyPushRadius"].AsSingle();
	private float NearbyPushAmount => FirearmStats.Additional["NearbyPushAmount"].AsSingle();

	public override void _Ready()
	{
		base._Ready();
		OnAttack += PushNearEnemies;
	}

	private void PushNearEnemies()
	{
		var playerPos = Player.GlobalPosition;
		if (!TargetQuery.TryGetTargetsInArea(playerPos, NearbyPushRadius, out var ids))
			return;

		foreach (var id in ids)
		{
			if (!GameWorld.World.IsAlive(id))
				continue;

			ref var posComponent = ref GameWorld.World.Get<PositionComponent>(id);

			var pushForce = playerPos.DirectionTo(posComponent.Position) * NearbyPushAmount;
			GameWorld.World.Set(id, new PositionComponent(posComponent.Position + pushForce));
		}
	}
}
