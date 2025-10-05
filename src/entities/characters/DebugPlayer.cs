using System.Collections.Generic;

namespace SurvivorsGame.Entities.Characters;

public partial class DebugPlayer : VBoxContainer
{
    private Label _healthLabel;
    private Label _itemsLabel;

    [Export] private Player _player;
    private Label _positionLabel;
    private Label _weaponLabel;
    [Export] public bool Enabled;
    [Export] public PlayerOffensiveController PlayerOffensiveController;
    [Export] public PlayerPassiveController PlayerPassiveController;

    public override void _Ready()
    {
        if (!Enabled)
        {
            Hide();
        }

        _healthLabel = GetChild<Label>(0);
        _positionLabel = GetChild<Label>(1);
        _weaponLabel = GetChild<Label>(2);
        _itemsLabel = GetChild<Label>(3);

        if (_player is null)
        {
            _healthLabel.QueueFree();
            _positionLabel.QueueFree();
        }

        if (PlayerPassiveController is null)
        {
            _itemsLabel.QueueFree();
        }
    }

    public override void _Process(double delta)
    {
        if (!Enabled)
        {
            return;
        }

        UpdateLabels();
    }

    private void UpdateLabels()
    {
        UpdateHealth();
        UpdatePosition();
        UpdatePassives();
        UpdateWeapons();
    }

    private void UpdateHealth()
    {
        _healthLabel.Text = "Health: " + _player.StatController.PlayerStats.Health;
    }

    private void UpdatePosition()
    {
        var position = $"({Math.Round(_player.Position.X)}, {Math.Round(_player.Position.Y)})";
        _positionLabel.Text = "Position: " + position;
    }

    private void UpdateWeapons()
    {
        if (PlayerOffensiveController is null)
        {
            return;
        }

        List<string> e = [];
        foreach (var weapon in PlayerOffensiveController.CurrentOffensives)
        {
            e.Add(weapon.Enabled
                ? $"{weapon.Properties.Name} ({weapon.Properties.CurrentLevel})"
                : $"!{weapon.Name} ({weapon.Properties.CurrentLevel})");
        }

        _weaponLabel.Text = "Weapons: " + string.Join(", ", e);
    }

    private void UpdatePassives()
    {
        if (PlayerPassiveController is null)
        {
            return;
        }

        List<string> e = [];
        foreach (var item in PlayerPassiveController.CurrentPassives)
        {
            e.Add(item.Applied
                ? $"{item.Properties.Name} ({item.Properties.CurrentLevel})"
                : $"!{item.Name} ({item.Properties.CurrentLevel})");
        }

        _itemsLabel.Text = "Items: " + string.Join(", ", e);
    }
}