// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Game.Levels.Controllers;
using Game.Levels.Controllers.Waves;
using Game.Players;

namespace Game.UI;

public partial class Hud : Node
{
	[Export]
	private Label? _playerHealthLabel;

	[Export]
	private Label? _enemyCountLabel;

	[Export]
	private Label? _moneyLabel;

	private Player Player => GameWorld.Instance.MainPlayer;
	private CharacterStats Stats => Player.Character.CharacterStats;
	private EnemyWaveController? WaveController => EnemyWaveController.Instance;
	private LevelData? LevelData => LevelData.Instance;

	public override void _Ready()
	{
		LevelData?.OnMoneyChanged += _ => _moneyLabel?.Text = $"${LevelData.Money}";
		_moneyLabel?.Text = $"${LevelData?.Money}";
	}

	public override void _Process(double delta)
	{
		_playerHealthLabel?.Text = "Health: " + Stats.Health;
		_enemyCountLabel?.Text = "Enemies: " + EnemyTracker.EnemyCount;
	}
}
