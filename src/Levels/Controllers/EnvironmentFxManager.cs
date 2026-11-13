// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Godot.Collections;

namespace Game.Levels.Controllers;

public partial class EnvironmentFxManager : Node
{
	[Signal]
	public delegate void OnSfxPlayedEventHandler(StringName sfxName, Dictionary<StringName, Variant> data);

	[Signal]
	public delegate void OnVfxPlayedEventHandler(StringName vfxName, Dictionary<StringName, Variant> data);

	public static EnvironmentFxManager? Instance;

	public override void _Ready()
	{
		Instance = this;
	}

	public override void _ExitTree()
	{
		Instance = null;
	}

	public static void PlaySfx(StringName sfxName, Dictionary<StringName, Variant>? data = null)
	{
		Instance?.PlaySfxInternal(sfxName, data);
	}

	public static void PlayVfx(StringName vfxName, Dictionary<StringName, Variant>? data = null)
	{
		Instance?.PlayVfxInternal(vfxName, data);
	}

	private void PlaySfxInternal(StringName sfxName, Dictionary<StringName, Variant>? data = null)
	{
		var dict = data ?? new Dictionary<StringName, Variant>();
		EmitSignalOnSfxPlayed(sfxName, dict);
	}

	private void PlayVfxInternal(StringName vfxName, Dictionary<StringName, Variant>? data = null)
	{
		var dict = data ?? new Dictionary<StringName, Variant>();
		EmitSignalOnVfxPlayed(vfxName, dict);
	}
}
