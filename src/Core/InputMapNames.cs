// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

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
	public static StringName NextItem { get; } = "next_item";
	public static StringName PreviousItem { get; } = "previous_item";
	public static StringName DebugFastForward { get; } = "DEBUG_FAST_FORWARD";
	public static readonly StringName UiCancel = "ui_cancel";
}
