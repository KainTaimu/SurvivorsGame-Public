// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Arch.Core;
using Game.Players;
using Schedulers;

namespace Game.Levels.Controllers;

[GlobalClass]
public partial class GameWorld : Node
{
	[Export]
	public Player MainPlayer { get; private set; } = null!;

	public static GameWorld Instance { get; private set; } = null!;

	public static World World { get; private set; } = null!;

	public const int MAX_ECS_ENTITIES = 16_384; // 2^14 = 16_384

	public override void _EnterTree()
	{
		World = World.Create(entityCapacity: MAX_ECS_ENTITIES);
		World.SharedJobScheduler = new JobScheduler(new JobScheduler.Config { ThreadPrefixName = "GameWorld" });
		Instance = this;
	}

	public override void _ExitTree()
	{
		World.SharedJobScheduler?.Dispose();
		World.Dispose();
		World = null!;
	}

	public override void _Ready()
	{
		GameWorldInstance.Instance.MainPlayer = MainPlayer;
	}
}
