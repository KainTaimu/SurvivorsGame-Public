using Newtonsoft.Json;
using SurvivorsGame.Systems;

namespace SurvivorsGame.UI.Menus;

public partial class PauseMenu : CanvasLayer
{
    [Export]
    private Label _playerStats;

    public PauseMenu()
    {
        if (Instance != null)
        {
            Logger.LogError("Cannot have multiple instances of a singleton!");
            QueueFree();
            return;
        }

        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
    }

    public static PauseMenu Instance { get; private set; }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true } keyEvent)
        {
            switch (keyEvent.Keycode)
            {
                case Key.Escape:
                    ToggleShow();
                    break;
            }
        }
    }

    private void ToggleShow()
    {
        var isPaused = PauseController.Instance.IsPaused;

        PauseController.Instance.Lock(this);
        switch (!isPaused)
        {
            case true:
                UpdatePlayerStats();
                Show();
                PauseController.Instance.Pause(this);
                break;

            case false:
                Hide();
                PauseController.Instance.Unpause(this);
                break;
        }

        PauseController.Instance.Unlock(this);
    }

    private void ShowSettings()
    {
    }

    private void ShowMainMenu()
    {
        GetTree().Quit();
    }

    private void UpdatePlayerStats()
    {
        if (GameWorld.Instance.MainPlayer.StatController is null)
        {
            return;
        }

        var playerStats = GameWorld.Instance.MainPlayer.StatController.PlayerStats;
        var statString = JsonConvert.SerializeObject(playerStats, Formatting.Indented);
        _playerStats.Text = statString;
    }
}