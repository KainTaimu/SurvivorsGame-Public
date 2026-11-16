// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Items.Offensive;

/// <summary>
/// Represents a reload behaviour that completes immediately and does not require ongoing processing.
/// </summary>
[GlobalClass]
public partial class DisposableReloadBehaviour : AbstractReloadBehaviour
{
	public override ReloadDisplayType DisplayType => ReloadDisplayType.Normal;

	public override void Reload()
	{
		EmitSignalOnReloadStart();
		EmitSignalOnReloadEnd();
	}

	public override void Process(float delta) { }
}
