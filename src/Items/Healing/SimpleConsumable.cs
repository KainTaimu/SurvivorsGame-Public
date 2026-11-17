// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Items.Healing;

public partial class SimpleConsumable : AbstractConsumable, IHasConsumeTimeStat
{
	[Export]
	public ConsumeTimeStat ConsumeTimeStat { get; private set; } = null!;

	[ExportCategory("")]
	[Export]
	public AbstractConsumptionBehaviour ConsumptionBehaviour { get; private set; } = null!;

	public override void Consume()
	{
		EmitSignalOnConsumed();
	}

	public override void Process(double delta)
	{
		ConsumptionBehaviour.Process((float)delta);
	}
}
