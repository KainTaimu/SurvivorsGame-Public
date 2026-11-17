// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Items.Offensive;

[GlobalClass]
public partial class MagazineStats : ItemStat
{
	[Signal]
	public delegate void OnMagazineCountChangedEventHandler(int neww, int old);

	[Export]
	public int Capacity
	{
		get;
		set
		{
			var old = field;
			field = value;
			EmitSignalOnMagazineCountChanged(field, old);
		}
	}

	public int Count
	{
		get;
		set
		{
			var old = field;
			field = Mathf.Clamp(value, 0, Capacity);
			EmitSignalOnMagazineCountChanged(field, old);
		}
	}

	public override void _EnterTree()
	{
		Count = Capacity;
	}
}
