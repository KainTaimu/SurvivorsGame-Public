using Game.Core.ECS;

namespace Game.Items.Offensive;

public partial class MarksmanRifle : Sniper
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
			if (!ComponentStore.GetComponent<PositionComponent>(id, out var posComponent))
				continue;

			var pushForce = playerPos.DirectionTo(posComponent.Position) * NearbyPushAmount;
			ComponentStore.SetComponent(id, new PositionComponent(posComponent.Position + pushForce));
		}
	}
}
