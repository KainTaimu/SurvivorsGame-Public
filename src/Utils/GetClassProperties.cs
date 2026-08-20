using System.Collections;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Game.Utils;

public static class ClassInspector
{
	private const int _floatingPointDisplayPrecision = 4;

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
				case float f:
					value = f.ToString("F" + _floatingPointDisplayPrecision);
					break;
				case double d:
					value = d.ToString("F" + _floatingPointDisplayPrecision);
					break;
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
			var fieldName = field.Name;
			if (fieldName is "NativePtr")
				continue;
			if (fieldName.StartsWith('<') && fieldName.Contains('>'))
			{
				var closingArrowIdx = fieldName.IndexOf('>');
				fieldName = fieldName[1..closingArrowIdx];
			}

			var value = field.GetValue(obj);
			switch (value)
			{
				case null:
					continue;
				case float f:
					value = f.ToString("F" + _floatingPointDisplayPrecision);
					break;
				case double d:
					value = d.ToString("F" + _floatingPointDisplayPrecision);
					break;
				case Node node:
					value = node.Name;
					break;
				case ICollection collection:
					value = GetCollectionPrettyString(collection);
					break;
			}

			s.AppendLine($"{ConvertPascalToTitleCase(fieldName)}: {value}");
		}

		return s.ToString();
	}

	private static string GetCollectionPrettyString(ICollection enumerable)
	{
		var b = new StringBuilder();
		b.Append('[');

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
				b.Append($", {node.Name}");
			else
				b.Append($", {item}");
			count++;
		}

		b.Append(']');
		return b.ToString();
	}

	private static string ConvertPascalToTitleCase(string input)
	{
		return string.IsNullOrEmpty(input) ? input : Regex.Replace(input, @"(?<!^)\p{Lu}", " $&");
	}
}
