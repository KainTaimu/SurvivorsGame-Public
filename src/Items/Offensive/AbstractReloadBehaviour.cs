namespace Game.Items.Offensive;

[Tool]
[GlobalClass]
public abstract partial class AbstractReloadBehaviour : Resource
{
	[Signal]
	public delegate void OnReloadStartEventHandler();

	[Signal]
	public delegate void OnReloadEndEventHandler();

	[Signal]
	public delegate void OnReloadProgressEventHandler(int progress, int maxProgress);

	public void Reload()
	{
		ReloadInternal();
		EmitSignalOnReloadStart();
	}

	private protected abstract void ReloadInternal();

	protected AbstractReloadBehaviour()
	{
		ResourceLocalToScene = true;
	}
}
