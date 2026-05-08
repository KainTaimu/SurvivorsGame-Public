using System.Diagnostics;
using System.Runtime.CompilerServices;

public static class LogTimer
{
    public static IDisposable LogTimeMsec(string name)
    {
        var start = Time.GetTicksMsec();
        return new DelegateDisposable(() =>
        {
            var elapsedMsec = Time.GetTicksMsec() - start;
            Logger.LogDebug($"{name} took {elapsedMsec:0.###} ms");
        });
    }

    public static IDisposable LogTimeMsec()
    {
        var name = GetCallerName();
        var start = Time.GetTicksMsec();
        return new DelegateDisposable(() =>
        {
            var elapsedMsec = Time.GetTicksMsec() - start;
            Logger.LogDebug($"{name} took {elapsedMsec:0.###} ms");
        });
    }

    public static IDisposable LogTimeUsec(string name)
    {
        var start = Time.GetTicksUsec();
        return new DelegateDisposable(() =>
        {
            var elapsedMsec = Time.GetTicksUsec() - start;
            Logger.LogDebug($"{name} took {elapsedMsec:0.###} us");
        });
    }

    public static IDisposable LogTimeUsec()
    {
        var name = GetCallerName();
        var start = Time.GetTicksUsec();
        return new DelegateDisposable(() =>
        {
            var elapsedMsec = Time.GetTicksUsec() - start;
            Logger.LogDebug($"{name} took {elapsedMsec:0.###} us");
        });
    }

    private sealed class DelegateDisposable(Action onDispose) : IDisposable
    {
        public void Dispose()
        {
            onDispose();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string GetCallerName()
    {
        var stackTrace = new StackTrace(1, false); //Captures 1 frame, false for not collecting information about the file
        var type = stackTrace.GetFrame(1)?.GetMethod()?.DeclaringType;

        return type is null ? "" : type.Name;
    }
}
