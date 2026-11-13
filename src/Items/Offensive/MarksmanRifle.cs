// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

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
