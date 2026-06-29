using System.Text;
using Game.Utils;
using Godot.Collections;

namespace Game.Items;

public enum ItemType
{
	Offensive,
	Passive,
}

[GlobalClass]
public partial class BaseItemStats : Resource
{
	[Export]
	public Dictionary<string, Variant> Additional
	{
		get;
		set => field = value;
	} = [];

	// TODO: Make proper way of getting formatted string without using reflection
	public string ToFormattedString()
	{
		var fields = ClassInspector.GetClassFieldsString(this);
		var sbuilder = new StringBuilder();
		foreach (var (key, value) in Additional)
			sbuilder.AppendLine($"{key}: {value}");

		var s = $"{fields}{sbuilder}";
		return s;
	}
}
