// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Game.Levels.Controllers;
using Game.Players;
using Game.Players.Controllers;

namespace Game.Items.Offensive.Components;

[GlobalClass]
public partial class StatusEffectComponent : Node
{
	[Export]
	private StatusEffect _statusEffect = null!;
	private bool _isStatusEffectApplied;

	private static Player Player => GameWorld.Instance.MainPlayer;

	public void GiveStatusEffect()
	{
		if (_isStatusEffectApplied)
			return;

		Player.StatusEffectController.AddStatusEffect(_statusEffect);
		_isStatusEffectApplied = true;
	}

	public void RemoveStatusEffect()
	{
		if (!_isStatusEffectApplied)
			return;

		Player.StatusEffectController.RemoveStatusEffect(_statusEffect);
		_isStatusEffectApplied = false;
	}
}
