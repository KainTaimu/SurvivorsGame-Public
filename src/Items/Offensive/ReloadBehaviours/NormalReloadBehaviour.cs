// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Items.Offensive;

[GlobalClass]
public partial class NormalReloadBehaviour : AbstractReloadBehaviour
{
	[Export]
	private MagazineStats MagazineStats { get; set; } = null!;

	[Export]
	private ReloadStats ReloadStats { get; set; } = null!;

	private float ReloadTime => ReloadStats.ReloadTime;

	private float _timeUntilFinishedReloading;

	public override ReloadDisplayType DisplayType => ReloadDisplayType.Normal;

	public override void Reload()
	{
		if (IsReloading)
			return;

		IsReloading = true;
		EmitSignalOnReloadStart();
		_timeUntilFinishedReloading = ReloadTime;
	}

	public override void Process(float delta)
	{
		if (!IsReloading)
			return;

		_timeUntilFinishedReloading -= delta;
		if (_timeUntilFinishedReloading > 0)
			return;

		EmitSignalOnReloadEnd();
		IsReloading = false;
		MagazineStats.Count = MagazineStats.Capacity;
	}
}
