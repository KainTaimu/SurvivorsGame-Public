using SurvivorsGame.Systems;

namespace SurvivorsGame.Levels.Benchmark;

public partial class BenchmarkRecorder : Node
{
    [Export]
    private int _enemyLimit = 300;

    private double _fpsCheckCooldown = 5;

    private bool _hasHitEnemyLimit;

    private bool _hasLoggedStats;

    private bool _hasStoppedWave;

    private double _waitTimeAfterStop = 5;

    private static GameWorld GwInstance => GameWorld.Instance;

    public override void _EnterTree()
    {
        Logger.LogDebug($"Stopping at {_enemyLimit}");
    }

    public override void _Process(double delta)
    {
        _fpsCheckCooldown -= delta;
        if (_fpsCheckCooldown > 0)
        {
            return;
        }

        if (GwInstance.CurrentLevel.WaveController.TotalEnemiesSpawned >= _enemyLimit)
        {
            if (!_hasStoppedWave)
            {
                GwInstance.CurrentLevel.WaveController.StopWave();
            }

            _waitTimeAfterStop -= delta;
        }

        if (_waitTimeAfterStop <= 0 && !_hasLoggedStats)
        {
            LogPerformanceStats();
        }
    }

    private void LogPerformanceStats()
    {
        var perf = Performance.Singleton;
        var timeElapsed = GwInstance.TimeElapsed;
        var enemyCount = GwInstance.Enemies.Count;
        var fps = Engine.GetFramesPerSecond();
        var ftime = perf.GetMonitor(Performance.Monitor.TimeProcess);
        var fFrameObjs = perf.GetMonitor(Performance.Monitor.RenderTotalObjectsInFrame);
        var fDrawCalls = perf.GetMonitor(Performance.Monitor.RenderTotalDrawCallsInFrame);
        var mem = perf.GetMonitor(Performance.Monitor.MemoryStatic) * 1e-6;
        var fVMem = perf.GetMonitor(Performance.Monitor.RenderVideoMemUsed) * 1e-6;

        Logger.LogDebug(
            $"Hit <{fps}FPS ({ftime * 1000:F1}ms) @ {timeElapsed} : {enemyCount} enemies"
        );
        Logger.LogDebug($"MEM: SYS({mem:F0}MB) GPU({fVMem:F0}MB)");
        Logger.LogDebug($"GPU: Frame Objs ({fFrameObjs}) : Frame Draw Calls ({fDrawCalls})");
        _hasLoggedStats = true;
    }
}
