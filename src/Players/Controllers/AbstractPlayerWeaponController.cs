// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Generic;
using Game.Items.Offensive;

namespace Game.Players.Controllers;

public abstract partial class AbstractPlayerWeaponController : Node
{
	[Signal]
	public delegate void OnOffensiveListChangedEventHandler(BaseOffensive newOffensive);

	[Signal]
	public delegate void OnPrimaryAttackReassignedEventHandler(BaseOffensive? neww, BaseOffensive? old);

	public BaseOffensive? PrimaryAttack
	{
		get;
		protected set
		{
			var old = field;
			field?.EmitSignal(BaseOffensive.SignalName.OnUnequipped);
			value?.EmitSignal(BaseOffensive.SignalName.OnEquipped);
			field = value;
			EmitSignalOnPrimaryAttackReassigned(value, old);
		}
	}

	public LinkedList<BaseOffensive> Offensives => OffensiveList;

	protected readonly LinkedList<BaseOffensive> OffensiveList = [];
}
