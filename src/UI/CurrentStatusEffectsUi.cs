// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Game.Players.Controllers;

namespace Game.UI;

public partial class CurrentStatusEffectsUi : CanvasLayer
{
	[Export]
	private PlayerStatusEffectController _statusEffectController = null!;

	[Export]
	private PackedScene _labelScene = null!;

	[Export]
	private Control _labelControl = null!;

	private readonly Dictionary<StatusEffect, Label> _labels = [];

	public override void _Ready()
	{
		_statusEffectController.OnStatusEffectApplied += effect =>
		{
			if (effect.Hidden && OS.HasFeature("prod"))
				return;
			var label = _labelScene.Instantiate<Label>();

			label.Text = GetStatusEffectText(effect);
			_labelControl.AddChild(label);
			_labels.Add(effect, label);
		};
		_statusEffectController.OnStatusEffectRemoved += effect =>
		{
			if (effect.Hidden && OS.HasFeature("prod"))
				return;
			var label = _labels[effect];
			_labels.Remove(effect);
			label.QueueFree();
		};
	}

	public override void _Process(double delta)
	{
		foreach (var (effect, label) in _labels)
			label.Text = GetStatusEffectText(effect);
	}

	private static string GetStatusEffectText(StatusEffect effect)
	{
		var debugTxt = effect.Hidden ? "[HIDDEN] " : "";
		return effect.Permanent
			? $"{debugTxt}{effect.Name} | Permanent"
			: $"{debugTxt}{effect
				.Name} " + $"| {effect
					.Duration:F1}";
	}
}
