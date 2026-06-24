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
			var label = _labelScene.Instantiate<Label>();
			label.Text = effect.Permanent ? $"{effect.Name} | Permanent" : $"{effect.Name} | {effect.Duration:F1}";
			_labelControl.AddChild(label);
			_labels.Add(effect, label);
		};
		_statusEffectController.OnStatusEffectRemoved += effect =>
		{
			var label = _labels[effect];
			_labels.Remove(effect);
			label.QueueFree();
		};
	}

	public override void _Process(double delta)
	{
		foreach (var (effect, label) in _labels)
			label.Text = effect.Permanent ? $"{effect.Name} | Permanent" : $"{effect.Name} | {effect.Duration:F1}";
	}
}
