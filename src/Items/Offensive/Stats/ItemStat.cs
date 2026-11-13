using Game.Utils;

namespace Game.Items.Offensive;

[GlobalClass]
public abstract partial class ItemStat : Node
{
	public override string ToString()
	{
		return ClassInspector.GetClassPropertiesString(this);
	}
}
