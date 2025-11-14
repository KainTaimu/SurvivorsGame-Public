using System.Collections.Generic;
using SurvivorsGame.Items.Offensive;
using SurvivorsGame.Systems;

namespace SurvivorsGame.Entities.Characters;

public partial class PlayerOffensiveController : Node
{
    private Player _owner;

    [Export]
    public bool Enabled = true;

    public List<BaseOffensive> CurrentOffensives { get; } = [];

    public override void _Ready()
    {
        InitializeWeaponController();
        AddInitialOffensiveItems();
    }

    public void AddWeapon(BaseOffensive offensive)
    {
        // Try to upgrade if offensive already exists. Otherwise, add as child.
        var x = CurrentOffensives.Find(x => x.Properties.Name == offensive.Properties.Name);
        if (x != null)
        {
            x.TryUpgrade();
            return;
        }

        // BUG: Adding as child may fail if offensive already has a parent
        AddChild(offensive);
        offensive.Initialize();
        CurrentOffensives.Add(offensive);
        Logger.LogDebug($"Added {offensive.Properties.Name}");
    }

    private void InitializeWeaponController()
    {
        _owner = GameWorld.Instance.MainPlayer;
        GameWorld.Instance.PlayerDied += OnPlayerDied;
    }

    private void AddInitialOffensiveItems()
    {
        foreach (var node in GetChildren())
        {
            var nodeType = node.GetType();
            if (node is not BaseOffensive offensive)
            {
                GD.PrintErr(
                    $"A child of {Name} (\"{node.Name}\") is not a BaseOffensive node. ({nodeType})"
                );
                node.QueueFree();
                return;
            }

            offensive.Initialize();
            CurrentOffensives.Add(offensive);
            offensive.Reparent(this, false);
        }
    }

    private void OnPlayerDied()
    {
        Enabled = false;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventKey { Pressed: true } keyEvent)
        {
            return;
        }

        PackedScene scene = null;
        BaseOffensive offensive = null;
        switch (keyEvent.Keycode)
        {
            case Key.Kp1:
                scene = GD.Load<PackedScene>("uid://dyia7od5wyo4h");
                offensive = scene.Instantiate<Pistol>();
                break;

            case Key.Kp2:
                scene = GD.Load<PackedScene>("uid://i30dnsyjcsua");
                offensive = scene.Instantiate<Shotgun>();
                break;

            case Key.Kp3:
                scene = GD.Load<PackedScene>("uid://c4qjuak0aya88");
                offensive = scene.Instantiate<Raygun>();
                break;
        }

        if (scene is not null)
        {
            AddWeapon(offensive);
        }
    }
}

