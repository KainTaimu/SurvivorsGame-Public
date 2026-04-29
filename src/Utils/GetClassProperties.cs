using System.Text;

namespace Game.Utils;

public static class GetClassProperties
{
	public static string GetClassPropertiesString(object obj)
	{
		var s = new StringBuilder();

		var pType = obj.GetType();
		var properties = pType.GetProperties(
			System.Reflection.BindingFlags.Public
				| System.Reflection.BindingFlags.NonPublic
				| System.Reflection.BindingFlags.Instance
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

	public static string GetClassFieldsString(object obj)
	{
		var s = new StringBuilder();

		var pType = obj.GetType();
		var fields = pType.GetFields(
			System.Reflection.BindingFlags.Public
				| System.Reflection.BindingFlags.NonPublic
				| System.Reflection.BindingFlags.Instance
		);
		foreach (var field in fields)
		{
			var value = field.GetValue(obj);
			if (value is null)
				continue;
			s.AppendLine($"{field.Name}: {value}");
		}
		return s.ToString();
	}
}
