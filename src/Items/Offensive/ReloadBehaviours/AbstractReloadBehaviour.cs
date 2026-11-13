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
