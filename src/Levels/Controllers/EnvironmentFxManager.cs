namespace Game.Levels.Controllers;

public partial class EnvironmentFxManager : Node
{
	[Signal]
	public delegate void OnSfxPlayedEventHandler(StringName sfxName);

	[Signal]
	public delegate void OnVfxPlayedEventHandler(StringName vfxName);

	public static EnvironmentFxManager? Instance;

	public override void _Ready()
	{
		Instance = this;
	}

	public override void _ExitTree()
	{
		Instance = null;
	}

	public static void PlaySfx(StringName sfxName)
	{
		Instance?.PlaySfxInternal(sfxName);
	}

	public static void PlayVfx(StringName vfxName)
	{
		Instance?.PlayVfxInternal(vfxName);
	}

	private void PlaySfxInternal(StringName sfxName)
	{
		EmitSignalOnSfxPlayed(sfxName);
	}

	private void PlayVfxInternal(StringName vfxName)
	{
		EmitSignalOnSfxPlayed(vfxName);
	}
}
