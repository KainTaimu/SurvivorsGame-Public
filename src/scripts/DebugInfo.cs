using System.Collections.Generic;
using System.Linq;
using SurvivorsGame.Systems;

public static class FpsTracker
{
    private static readonly Queue<double> FpsHistory;
    private static readonly int MaxSamples;
    private static double _totalFps;

    static FpsTracker() // Default to 1 second worth of samples at 60fps
    {
        MaxSamples = 60;
        FpsHistory = new Queue<double>(MaxSamples);
        _totalFps = 0;
    }

    public static void Update()
    {
        // Get current FPS using Godot's Performance monitor
        var currentFps = Performance.GetMonitor(Performance.Monitor.TimeFps);

        // Add new sample
        FpsHistory.Enqueue(currentFps);
        _totalFps += currentFps;

        // Remove oldest sample if we exceed max samples
        if (FpsHistory.Count > MaxSamples)
        {
            _totalFps -= FpsHistory.Dequeue();
        }
    }

    public static double GetAverageFps()
    {
        if (FpsHistory.Count == 0)
        {
            return 0;
        }

        return _totalFps / FpsHistory.Count;
    }

    // Get minimum FPS in the sample window
    public static double GetMinFps()
    {
        return FpsHistory.Count > 0 ? FpsHistory.Min() : 0;
    }

    // Get maximum FPS in the sample window
    public static double GetMaxFps()
    {
        return FpsHistory.Count > 0 ? FpsHistory.Max() : 0;
    }
}

public partial class DebugInfo : CanvasLayer
{
    [Export] private bool _enabled;
    [Export] private Label _enemyLabel;
    [Export] private Label _fpsLabel;
    [Export] private Label _healthLabel;
    [Export] private Label _levelLabel;
    [Export] private Label _xpLabel;
    [Export] private Label _screenLabel;
    [Export] private Label _timeElapsed;

    [Export] private Timer _timer;
    [Export] private float _updateInterval = 0.05f;

    public override void _Ready()
    {
        _timer.WaitTime = _updateInterval;
        _timer.Start();

        if (!_enabled)
        {
            Hide();
        }
    }

    public override void _Process(double delta)
    {
    }

    private void OnTimerTimeout()
    {
        if (!_enabled)
        {
            return;
        }

        UpdateDebugInfo();
    }

    private void UpdateDebugInfo()
    {
        FpsTracker.Update();
        var currentFps = Engine.GetFramesPerSecond();
        var averageFps = FpsTracker.GetAverageFps();
        var minFps = FpsTracker.GetMinFps();
        var maxFps = FpsTracker.GetMaxFps();

        var enemyCount = GameWorld.Instance.Enemies.Count;
        var playerHealth = GameWorld.Instance.MainPlayer?.StatController.PlayerStats.Health.ToString() ?? "N/A";
        var playerXp = GameWorld.Instance.MainPlayer?.XpController.Xp.ToString() ?? "N/A";
        var playerLevel = GameWorld.Instance.MainPlayer?.XpController.Level.ToString() ?? "N/A";
        var waveStats = GameWorld.Instance.CurrentLevel.WaveController?.GetWaveStats() ?? "N/A";

        _fpsLabel.Text = $"FPS: {currentFps} (Avg: {averageFps:F1} | Min: {minFps:F1} | Max: {maxFps:F1})";
        _enemyLabel.Text = $"Enemies: {enemyCount}";
        _healthLabel.Text = $"Health: {playerHealth}";
        _xpLabel.Text = $"Xp: {playerXp}";
        _levelLabel.Text = $"Level: {playerLevel}";
        _screenLabel.Text = $"Screen: {GetViewport().GetVisibleRect().Size}";
        _timeElapsed.Text = $"Time elapsed: {GameWorld.Instance.TimeElapsed} ({waveStats})";
    }
}