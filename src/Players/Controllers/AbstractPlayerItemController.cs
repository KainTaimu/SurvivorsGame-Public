// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Generic;
using Game.Items;

namespace Game.Players.Controllers;

public abstract partial class AbstractPlayerItemController : Node
{
	[Signal]
	public delegate void OnItemListChangedEventHandler(BaseItem newItem);

	[Signal]
	public delegate void OnEquippedReassignedEventHandler(BaseItem? neww, BaseItem? old);

	public BaseItem? Equipped
	{
		get;
		protected set
		{
			var old = field;
			field?.EmitSignal(BaseItem.SignalName.OnUnequipped);
			value?.EmitSignal(BaseItem.SignalName.OnEquipped);
			field = value;
			EmitSignalOnEquippedReassigned(value, old);
		}
	}

	public LinkedList<BaseItem> Items => ItemList;

	protected readonly LinkedList<BaseItem> ItemList = [];
}
