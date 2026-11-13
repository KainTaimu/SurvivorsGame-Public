// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Collections;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Game.Items.Offensive;

namespace Game.Utils;

public static class ClassInspector
{
	private const int FLOATING_POINT_DISPLAY_PRECISION = 4;

	public static string GetClassPropertiesString(
		object obj,
		BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
		int recursionDepth = 0
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

			var indent = new string('\t', recursionDepth + 1);
			var value = property.GetValue(obj);
			if (FormatPropertyValue(ref value, recursionDepth))
				continue;

			s.AppendLine($"{indent}{property.Name}: {value}");
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

			var indent = new string('\t', recursionDepth + 1);
			var value = field.GetValue(obj);
			if (FormatPropertyValue(ref value, recursionDepth))
				continue;

			s.AppendLine($"{indent}{ConvertPascalToTitleCase(fieldName)}: {value}");
		}

		return s.ToString();
	}

	private static bool FormatPropertyValue(
		ref object? value,
		int recursionDepth = 0,
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
			case ItemStat stat:
				value = stat.ToString();
				break;
			case Node node:
				value = node.Name;
				break;
			case Resource resource when recursionDepth > 0:
				value =
					$"{{\n{GetClassFieldsString(resource, flags, recursionDepth - 1)},\n{GetClassPropertiesString(resource, flags, recursionDepth - 1)}}}";
				break;
			case Resource resource:
				value = resource.ResourceName;
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
