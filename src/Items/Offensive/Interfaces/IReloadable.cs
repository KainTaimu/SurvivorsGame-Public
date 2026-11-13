// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Items.Offensive;

public interface IReloadable
{
	int MagazineCapacity { get; }
	int MagazineCount { get; }
	bool IsReloading { get; }
}
