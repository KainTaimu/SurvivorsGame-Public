// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

using Game.Levels.Controllers;
using Game.Players;
using Game.Players.Controllers;

namespace Game.Items.Offensive.Modules;

[GlobalClass]
public partial class StatusEffectModule : Node
{
	[Export]
	private StatusEffect _statusEffect = null!;

	[Export]
	private bool _isEffectSingleInstance = true;

	private bool _isStatusEffectApplied;

	private static Player Player => GameWorld.Instance.MainPlayer;

	public void GiveStatusEffect()
	{
		if (_isEffectSingleInstance && _isStatusEffectApplied)
			return;

		if (_isEffectSingleInstance)
			Player.StatusEffectController.AddStatusEffect(_statusEffect);
		else
			Player.StatusEffectController.AddStatusEffect((StatusEffect)_statusEffect.Duplicate(true));
		_isStatusEffectApplied = true;
	}

	// NOTE: if _isEffectSingleInstance, nothing happens because _statusEffect is duplicated
	public void RemoveStatusEffect()
	{
		if (!_isEffectSingleInstance || !_isStatusEffectApplied)
			return;

		Player.StatusEffectController.RemoveStatusEffect(_statusEffect);
		_isStatusEffectApplied = false;
	}
}
