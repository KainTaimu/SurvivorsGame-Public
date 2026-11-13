// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

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
