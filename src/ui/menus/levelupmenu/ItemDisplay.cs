using System.Collections.Generic;
using SurvivorsGame.Entities.Characters;
using SurvivorsGame.Items.Passive;
using SurvivorsGame.Systems;

namespace SurvivorsGame.UI.Menus;

public partial class ItemDisplay : Control
{
    private PlayerPassiveController PlayerPassiveController => GameWorld.Instance.MainPlayer.PassiveController;
    private List<BasePassive> Items => PlayerPassiveController.CurrentPassives;
    [Export] private PackedScene _itemContainerScene;
    [Export] private HBoxContainer _boxContainer;

    private void OnPaused()
    {
        foreach (var item in Items)
        {
            var newNode = _itemContainerScene.Instantiate();
            var label = newNode.GetNode<Button>("Button");
            label.Text = item.Properties.Name;

            _boxContainer.AddChild(newNode);
        }
    }

    private void OnUnpaused()
    {
        foreach (var child in _boxContainer.GetChildren())
        {
            child.QueueFree();
        }
    }
}