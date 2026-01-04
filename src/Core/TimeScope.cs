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

    public static IDisposable LogTimeUsec(string name)
    {
        var start = Time.GetTicksUsec();
        return new DelegateDisposable(() =>
        {
            var elapsedMsec = Time.GetTicksUsec() - start;
            Logger.LogDebug($"{name} took {elapsedMsec:0.###} us");
        });
    }

    sealed class DelegateDisposable(Action onDispose) : IDisposable
    {
        public void Dispose() => onDispose();
    }
}
