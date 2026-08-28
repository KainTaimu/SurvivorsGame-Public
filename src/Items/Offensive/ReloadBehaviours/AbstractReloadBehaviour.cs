namespace Game.Items.Offensive;

public enum ReloadDisplayType
{
	Normal,
	Progressive,
}

[Tool]
[GlobalClass]
public abstract partial class AbstractReloadBehaviour : Resource
{
	[Signal]
	public delegate void OnReloadStartEventHandler();

	[Signal]
	public delegate void OnReloadEndEventHandler();

	[Signal]
	public delegate void OnReloadEndInterruptedEventHandler();

	[Signal]
	public delegate void OnReloadProgressEventHandler(int progress, int maxProgress, int step);

	[Export]
	public ReloadDisplayType DisplayType = ReloadDisplayType.Normal;

	public bool IsReloading { get; protected set; }

	public abstract void Reload();

	public abstract void Process(float delta);

	protected AbstractReloadBehaviour()
	{
		ResourceLocalToScene = true;
	}
}
