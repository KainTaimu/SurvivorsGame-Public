// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace Game;

public static class InputMapNames
{
	public static StringName MoveUp { get; } = "move_up";
	public static StringName MoveDown { get; } = "move_down";
	public static StringName MoveLeft { get; } = "move_left";
	public static StringName MoveRight { get; } = "move_right";
	public static StringName Sprint { get; } = "sprint";
	public static StringName PrimaryAttack { get; } = "primary_attack";
	public static StringName SecondaryAttack { get; } = "secondary_attack";
	public static StringName WeaponReload { get; } = "weapon_reload";
	public static StringName Interact { get; } = "interact";
	public static StringName NextWeapon { get; } = "next_weapon";
	public static StringName PreviousWeapon { get; } = "previous_weapon";
	public static readonly StringName UiCancel = "ui_cancel";
}
