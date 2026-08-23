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
