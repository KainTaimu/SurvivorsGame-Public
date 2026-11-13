// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace Game.Items.Offensive;

public enum ReloadDisplayType
{
	/// <summary>
	/// Displays "Reloading..." prompt
	/// </summary>
	Normal,

	/// <summary>
	/// Always displays ammo count
	/// </summary>
	Progressive,
}

[GlobalClass]
public abstract partial class AbstractReloadBehaviour : Node
{
	[Signal]
	public delegate void OnReloadStartEventHandler();

	[Signal]
	public delegate void OnReloadEndEventHandler();

	[Signal]
	public delegate void OnReloadEndInterruptedEventHandler();

	[Signal]
	public delegate void OnReloadProgressEventHandler(int progress, int maxProgress, int step);

	public abstract ReloadDisplayType DisplayType { get; }

	public bool IsReloading { get; protected set; }

	public abstract void Reload();

	public abstract void Process(float delta);
}
