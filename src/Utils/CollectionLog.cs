using System.Collections;
using System.Text;

namespace Game.Utils;

public static class PrettyCollection
{
    public static string GetPrettyDictionary(IDictionary d)
    {
        var b = new StringBuilder();
        b.Append("{");

        for (var i = 0; i < d.Count - 1; i++)
        {
            b.Append($"({d.Keys.ToString()} : {d.Values.ToString()}), ");
        }
        b.Append($"({d.Keys.ToString()} : {d.Values.ToString()})");

        b.Append("}");

        return b.ToString();
    }
}
