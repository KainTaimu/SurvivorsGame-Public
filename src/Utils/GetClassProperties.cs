using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Game.Utils;

public static class ClassInspector
{
	private const int FLOATING_POINT_DISPLAY_PRECISION = 4;

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
			var propertyName = property.Name;
			if (!FormatPropertyName(ref propertyName))
				continue;

			var value = property.GetValue(obj);
			if (FormatPropertyValue(ref value))
				continue;

			s.AppendLine($"{property.Name}: {value}");
		}

		return s.ToString();
	}

	public static string GetClassFieldsString(
		object obj,
		BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
		int recursionDepth = 0
	)
	{
		var s = new StringBuilder();

		var pType = obj.GetType();
		var fields = pType.GetFields(flags);
		foreach (var field in fields)
		{
			var fieldName = field.Name;
			if (!FormatPropertyName(ref fieldName))
				continue;
			if (fieldName.StartsWith('<') && fieldName.Contains('>'))
			{
				var closingArrowIdx = fieldName.IndexOf('>');
				fieldName = fieldName[1..closingArrowIdx];
			}

			var value = field.GetValue(obj);
			if (FormatPropertyValue(ref value))
				continue;

			s.AppendLine($"{ConvertPascalToTitleCase(fieldName)}: {value}");
		}

		return s.ToString();
	}

	private static bool FormatPropertyValue(
		ref object? value,
		BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
	)
	{
		switch (value)
		{
			case null:
				return true;
			case float f:
				value = f.ToString("F" + FLOATING_POINT_DISPLAY_PRECISION);
				break;
			case double d:
				value = d.ToString("F" + FLOATING_POINT_DISPLAY_PRECISION);
				break;
			case Node node:
				value = node.Name;
				break;
			case Resource resource:
				value = $"{{\n{GetClassFieldsString(resource, flags)},\n{GetClassPropertiesString(resource, flags)}}}";
				break;
			case ICollection collection:
				value = GetCollectionPrettyString(collection);
				break;
		}

		return false;
	}

	private static bool FormatPropertyName(ref string s)
	{
		switch (s)
		{
			case "_Bundled":
			case "NativePtr":
			case "ResourceLocalToScene":
			case "ResourcePath":
			case "ResourceName":
			case "ResourceSceneUniqueId":
			case "NativeInstance":
				return false;
		}
		return true;
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
