// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

using Game.Core.ECS;
using Game.Levels.Controllers;

namespace Game.Items.Offensive;

public sealed partial class MarksmanRifle : Sniper, IHasRadialPushStats
{
	[Export]
	public RadialPushStats RadialPushStats { get; set; } = null!;

	public override void _Ready()
	{
		base._Ready();
		OnAttack += PushNearEnemies;
	}

	private void PushNearEnemies()
	{
		var playerPos = Player.GlobalPosition;
		if (!TargetQuery.TryGetTargetsInArea(playerPos, RadialPushStats.NearbyPushRadius, out var ids))
			return;

		foreach (var id in ids)
		{
			if (!GameWorld.World.IsAlive(id))
				continue;

			ref var posComponent = ref GameWorld.World.Get<PositionComponent>(id);

			var pushForce = playerPos.DirectionTo(posComponent.Position) * RadialPushStats.NearbyPushAmount;
			GameWorld.World.Set(id, new PositionComponent(posComponent.Position + pushForce));
		}
	}
}
