namespace Game.Items.Offensive;

[Tool]
[GlobalClass]
public abstract partial class AbstractFireGroup : Resource, IFireGroup
{
	[Signal]
	public delegate void OnFireEventHandler();

	public abstract void ProcessInput();

	protected AbstractFireGroup()
	{
		ResourceLocalToScene = true;
	}
}
