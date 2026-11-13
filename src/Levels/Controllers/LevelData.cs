// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Game.Levels.Controllers.Waves;

namespace Game.Levels.Controllers;

[GlobalClass]
public partial class LevelData : Node
{
	[Signal]
	public delegate void OnMoneyChangedEventHandler(int delta);

	[Export]
	public EnemyWaveController? WaveController { get; private set; }

	public int Money
	{
		get;
		set
		{
			var old = field;
			field = value;
			EmitSignalOnMoneyChanged(old - value);
		}
	}

	public static LevelData? Instance { get; private set; }

	public override void _Ready()
	{
		Instance = this;
		GameWorldInstance.Instance.LevelData = this;
	}
}
