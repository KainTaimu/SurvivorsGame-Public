// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Items.Healing;

[GlobalClass]
public partial class InjectorConsumptionBehaviour : AbstractConsumptionBehaviour
{
	[Export]
	private ConsumeTimeStat ConsumeTimeStat { get; set; } = null!;

	private double _heldTime;

	public override void _Process(double delta)
	{
		if (!Input.IsActionPressed(InputMapNames.PrimaryAttack))
			_heldTime = Mathf.Max(0, _heldTime - delta);
	}

	public override void Process(float delta)
	{
		if (!Input.IsActionPressed(InputMapNames.PrimaryAttack))
			return;

		_heldTime += delta;
		if (_heldTime < ConsumeTimeStat.TimeToConsume)
			return;

		EmitSignalOnConsumed();
	}
}
