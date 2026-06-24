using System.Collections;
using System.Reflection;
using System.Text;

namespace Game.Utils;

public static class ClassInspector
{
	public static string GetClassPropertiesString(
		object obj,
		BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
	)
	{
		var s = new StringBuilder();

		var pType = obj.GetType();
		var properties = pType.GetProperties(flags);
		foreach (var property in properties)
		{
			var value = property.GetValue(obj);
			switch (value)
			{
				case null:
					continue;
				case Node node:
					value = node.Name;
					break;
				case ICollection collection:
					value = GetCollectionPrettyString(collection);
					break;
			}

			s.AppendLine($"{property.Name}: {value}");
		}

		return s.ToString();
	}

	public static string GetClassFieldsString(
		object obj,
		BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
	)
	{
		var s = new StringBuilder();

		var pType = obj.GetType();
		var fields = pType.GetFields(flags);
		foreach (var field in fields)
		{
			if (field.Name == "NativePtr")
				continue;
			var value = field.GetValue(obj);
			switch (value)
			{
				case null:
					continue;
				case Node node:
					value = node.Name;
					break;
				case ICollection collection:
					value = GetCollectionPrettyString(collection);
					break;
			}

			s.AppendLine($"{field.Name}: {value}");
		}

		return s.ToString();
	}

	private static string GetCollectionPrettyString(ICollection enumerable)
	{
		var b = new StringBuilder();
		b.Append("[");

		var count = 0;
		const int maxItemsToShow = 5;
		foreach (var item in enumerable)
		{
			if (count > maxItemsToShow)
			{
				b.Append($"...{enumerable.Count - maxItemsToShow} more");
				break;
			}

			if (item is Node node)
				b.AppendFormat(", {0}", node.Name);
			else
				b.AppendFormat(", {0}", item);
			count++;
		}

		b.Append("]");
		return b.ToString();
	}
}
