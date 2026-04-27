using System.Text;

namespace Game.Utils;

public static class GetClassProperties
{
	extension(Node node)
	{
		public string GetClassPropertiesString()
		{
			var s = new StringBuilder();

			var pType = node.GetType();
			var properties = pType.GetProperties(
				System.Reflection.BindingFlags.Public
					| System.Reflection.BindingFlags.NonPublic
					| System.Reflection.BindingFlags.Instance
			);
			foreach (var property in properties)
			{
				var value = property.GetValue(node);
				if (value is null)
					continue;
				s.AppendLine($"{property.Name}: {value}");
			}
			return s.ToString();
		}

		public string GetClassFieldsString()
		{
			var s = new StringBuilder();

			var pType = node.GetType();
			var fields = pType.GetFields(
				System.Reflection.BindingFlags.Public
					| System.Reflection.BindingFlags.NonPublic
					| System.Reflection.BindingFlags.Instance
			);
			foreach (var field in fields)
			{
				var value = field.GetValue(node);
				if (value is null)
					continue;
				s.AppendLine($"{field.Name}: {value}");
			}
			return s.ToString();
		}
	}
}
