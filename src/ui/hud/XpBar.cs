using SurvivorsGame.Entities.Characters;
using SurvivorsGame.Systems;

namespace SurvivorsGame.Levels.Systems;

public partial class XpBar : ProgressBar
{
    private PlayerXpController _playerXpController;

    public override void _Ready()
    {
        _playerXpController = GameWorld.Instance.MainPlayer.XpController;
    }

    public override void _Process(double delta)
    {
        Value = _playerXpController.Xp;
        MaxValue = _playerXpController.XpCap;
    }
}

