// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Arch.Core;
using Game.Core.ECS;

namespace Game.Levels.Controllers;

[GlobalClass]
public partial class LungerEnemyBehavior : AbstractEnemyBehavior
{
	[Export]
	public float TimeBetweenLunges = 5;

	[Export]
	public float MinDistanceToLunge = 250;

	[Export]
	public float LungeVelocityMultiplier = 3.5f;

	public override void AddComponent(Entity entity)
	{
		GameWorld.World.Add(
			entity,
			new LungerBehaviorDataComponent(0, TimeBetweenLunges, MinDistanceToLunge, LungeVelocityMultiplier)
		);
	}
}
