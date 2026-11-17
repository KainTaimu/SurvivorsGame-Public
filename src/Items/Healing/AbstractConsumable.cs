// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Items.Healing;

public abstract partial class AbstractConsumable : BaseItem
{
	[Signal]
	public delegate void OnConsumedEventHandler();

	public abstract void Consume();
}
