using System.Collections.Generic;
using SurvivorsGame.Entities.Enemies.States;

namespace SurvivorsGame.Entities.Enemies;

public partial class EnemyDebug : VBoxContainer
{
    [Export] public bool Enabled;

    [Export] private BotStatController _botStatController;
    [Export] private BotEffectsController _effects;
    [Export] private BaseEnemy _owner;
    [Export] private StateMachine _state;

    [Export] private Label _healthLabel;
    [Export] private Label _speedLabel;
    [Export] private Label _stateLabel;
    [Export] private Label _affectByLabel;

    public override void _EnterTree()
    {
        if (!Enabled)
        {
            Hide();
        }
    }

    public override void _Process(double delta)
    {
        if (!Enabled)
        {
            return;
        }

        _healthLabel.Text = "Health: " + _botStatController.Health;
        _speedLabel.Text = "Speed: " + _botStatController.MoveSpeed;
        _stateLabel.Text = "State: " + _state.CurrentState.Name;

        List<string> e = new();
        for (var i = 0; i < _effects.CurrentEffects.Count; i++)
        {
            e.Add(_effects.CurrentEffects[i].ToString());
        }

        _affectByLabel.Text = "AffectedBy: " + string.Join(", ", e);
    }
}