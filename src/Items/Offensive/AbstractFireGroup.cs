namespace Game.Items.Offensive;

[GlobalClass]
public abstract partial class AbstractFireGroup : Resource, IFireGroup
{
	public abstract bool TryFire();
}
