namespace Game.Items.Offensive;

[GlobalClass]
public abstract partial class AbstractFireGroup : Node
{
	[Signal]
	public delegate void OnFireEventHandler();

	public abstract void ProcessInput();
}
