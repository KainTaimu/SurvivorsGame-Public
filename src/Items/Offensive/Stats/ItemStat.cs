// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

using Game.Utils;

namespace Game.Items.Offensive;

[GlobalClass]
public abstract partial class ItemStat : Node
{
	public override string ToString()
	{
		return ClassInspector.GetClassPropertiesString(this);
	}
}
