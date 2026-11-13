// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Items.Offensive;

[GlobalClass]
public abstract partial class AbstractFireGroup : Node
{
	[Signal]
	public delegate void OnFireEventHandler();

	public abstract void ProcessInput();
}
