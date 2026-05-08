using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

public static class Logger
{
    public static void LogInfo(params object[] s)
    {
        SendLog(
            GD.PrintRich,
            $"[color=white][Info  :  {GetCallerName()}] {ConstructString(s)}[/color]"
        );
    }

    public static void LogDebug(params object[] s)
    {
        SendLog(
            GD.PrintRich,
            $"[color=darkgray][Debug  :  {GetCallerName()}] {ConstructString(s)}[/color]"
        );
    }

    public static void LogWarning(params object[] s)
    {
        SendLog(
            GD.PrintRich,
            $"[color=yellow][Warning  :  {GetCallerName()}] {ConstructString(s)}[/color]"
        );
    }

    public static void LogError(params object[] s)
    {
        GD.PrintErr($"[Error  :  {GetCallerName()}] {ConstructString(s)}");
    }

    private static void SendLog(Action<string> method, string message)
    {
        method(message);
    }

    private static string ConstructString(object[] s)
    {
        var builder = new StringBuilder();
        foreach (var x in s)
            builder.Append($"{x} ");
        return builder.ToString();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string GetCallerName()
    {
        var stackTrace = new StackTrace(1, false); //Captures 1 frame, false for not collecting information about the file
        var type = stackTrace.GetFrame(1)?.GetMethod()?.DeclaringType;

        return type is null ? "" : type.Name;
    }
}
