// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Game.Players;

namespace Game.Levels.Controllers;

public partial class GameWorldInstance : Node
{
	public Player? MainPlayer { get; set; }
	public LevelData? LevelData { get; set; }

	public static GameWorldInstance Instance = null!;

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _ExitTree()
	{
		Instance = null!;
	}
}
