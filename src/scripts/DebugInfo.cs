using SurvivorsGame.Systems;

public partial class DebugInfo : CanvasLayer
{
    [Export]
    private bool _enabled;

    [Export]
    private Label _enemyLabel;

    [Export]
    private Label _healthLabel;

    [Export]
    private Label _levelLabel;

    [Export]
    private Label _screenLabel;

    [Export]
    private Label _timeElapsed;

    [Export]
    private Timer _timer;

    [Export]
    private float _updateInterval = 0.05f;

    [Export]
    private Label _xpLabel;

    public override void _Ready()
    {
        _timer.WaitTime = _updateInterval;
        _timer.Start();

        if (!_enabled)
        {
            Hide();
        }
    }

    public override void _Process(double delta) { }

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
        var enemyCount = GameWorld.Instance.Enemies.Count;
        var playerHealth =
            GameWorld.Instance.MainPlayer?.StatController.PlayerStats.Health.ToString() ?? "N/A";
        var playerXp = GameWorld.Instance.MainPlayer?.XpController.Xp.ToString() ?? "N/A";
        var playerLevel = GameWorld.Instance.MainPlayer?.XpController.Level.ToString() ?? "N/A";
        var waveStats = GameWorld.Instance.CurrentLevel.WaveController?.GetWaveStats() ?? "N/A";

        _enemyLabel.Text = $"Enemies: {enemyCount}";
        _healthLabel.Text = $"Health: {playerHealth}";
        _xpLabel.Text = $"Xp: {playerXp}";
        _levelLabel.Text = $"Level: {playerLevel}";
        _screenLabel.Text = $"Screen: {GetViewport().GetVisibleRect().Size}";
        _timeElapsed.Text = $"Time elapsed: {GameWorld.Instance.TimeElapsed} ({waveStats})";
    }
}

