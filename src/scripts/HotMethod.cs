/// <summary>
/// Marks a method as performance-intensive and must be used sparingly.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class HotMethod(string reason) : Attribute
{
    public string Reason = reason;
}
