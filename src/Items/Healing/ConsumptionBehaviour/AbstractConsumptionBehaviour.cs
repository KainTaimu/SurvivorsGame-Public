// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Items.Healing;

[GlobalClass]
public abstract partial class AbstractConsumptionBehaviour : Node
{
	[Signal]
	public delegate void OnConsumedEventHandler();

	public abstract void Process(float delta);
}
