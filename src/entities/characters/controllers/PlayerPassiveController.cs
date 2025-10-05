using System.Collections.Generic;
using System.Linq;
using SurvivorsGame.Items.Passive;
using SurvivorsGame.Systems;

namespace SurvivorsGame.Entities.Characters;

public partial class PlayerPassiveController : Node
{
    public List<BasePassive> CurrentPassives { get; } = [];

    private Player _player;

    public override void _Ready()
    {
        InitializePassiveController();
        AddInitialPassiveItems();
    }

    public override void _Process(double delta)
    {
        ProcessItems(delta);
    }

    public void AddItem(BasePassive passive)
    {
        // Try to upgrade if passive already exists. Otherwise, add as child.
        var x = CurrentPassives.Find(x => x.Properties.Name == passive.Properties.Name);
        if (x != null)
        {
            x.TryUpgrade();
            return;
        }

        // BUG: Adding as child may fail if passive already has a parent
        AddChild(passive);
        passive.Enter();
        CurrentPassives.Add(passive);
        Logger.LogDebug($"Added {passive.Properties.Name}");
    }

    public void RemoveItem(Type passive)
    {
        var x = CurrentPassives.FirstOrDefault(x => x.GetType() == passive);
        if (x is null)
        {
            return;
        }

        Logger.LogDebug($"Removed {x.Properties.Name}");
        CurrentPassives.Remove(x);
        x.QueueFree();
    }

    private void ProcessItems(double delta)
    {
        foreach (var item in CurrentPassives)
        {
            item.Apply(delta);
        }
    }

    private void InitializePassiveController()
    {
        _player = GameWorld.Instance.MainPlayer;
    }

    private void AddInitialPassiveItems()
    {
        foreach (var node in GetChildren())
        {
            var nodeType = node.GetType();
            if (node is not BasePassive passive)
            {
                GD.PrintErr($"A child of {Name} (\"{node.Name}\") is not a BasePassive node. ({nodeType})");
                node.QueueFree();
                return;
            }

            passive.Enter();
            CurrentPassives.Add(passive);
            passive.Reparent(this, false);
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventKey { Pressed: true } keyEvent)
        {
            return;
        }

        switch (keyEvent.Keycode)
        {
            case Key.Kp7:
                var scene = GD.Load<PackedScene>("uid://fo0uohokdxdk");
                BasePassive passive = scene.Instantiate<FlakJacket>();
                AddItem(passive);
                break;
            case Key.Kp8:
                RemoveItem(typeof(FlakJacket));
                break;

            // case Key.Kp9:
            //     AddItem("Flak Jacket");
            //     break;
        }
    }
}