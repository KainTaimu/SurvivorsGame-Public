using System.Diagnostics;
using System.Runtime.CompilerServices;
using SurvivorsGame.Systems;

public static class Logger
{
    public static void LogInfo(object message)
    {
        SendLog(GD.PrintRich, $"[color=white][Info  :  {GetCallerName()}] {message}[/color]");
    }

    public static void LogDebug(object message)
    {
        SendLog(GD.PrintRich, $"[color=darkgray][Debug  :  {GetCallerName()}] {message}[/color]");
    }

    public static void LogWarning(object message)
    {
        SendLog(GD.PrintRich, $"[color=yellow][Warning  :  {GetCallerName()}] {message}[/color]");
    }

    public static void LogError(object message)
    {
        GD.PrintErr($"[Error  :  {GetCallerName()}] {message}");
    }

    private static void SendLog(Action<string> method, string message)
    {
        if (!Game.IsDebugEnabled)
        {
            return;
        }

        method(message);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string GetCallerName()
    {
        var stackTrace =
            new StackTrace(1, false); //Captures 1 frame, false for not collecting information about the file
        var type = stackTrace.GetFrame(1)?.GetMethod()?.DeclaringType;

        return type is null ? "" : type.Name;
    }
}