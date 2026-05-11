using System.Text;

namespace Game.Utils;

public static class ClassInspector
{
	public static string GetClassPropertiesString(object obj, System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
	{
		var s = new StringBuilder();

		var pType = obj.GetType();
		var properties = pType.GetProperties(
			flags
		);
		foreach (var property in properties)
		{
			var value = property.GetValue(obj);
			if (value is null)
				continue;
			s.AppendLine($"{property.Name}: {value}");
		}

		return s.ToString();
	}

	public static string GetClassFieldsString(object obj, System.Reflection.BindingFlags flags = System.Reflection
			.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
	{
		var s = new StringBuilder();

		var pType = obj.GetType();
		var fields = pType.GetFields(
			flags
		);
		foreach (var field in fields)
		{
			if (field.Name == "NativePtr")
				continue;
			var value = field.GetValue(obj);
			if (value is null)
				continue;
			s.AppendLine($"{field.Name}: {value}");
		}

		return s.ToString();
	}
}
