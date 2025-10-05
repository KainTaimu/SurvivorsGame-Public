using SurvivorsGame.Entities.Characters;
using SurvivorsGame.Items;
using SurvivorsGame.Items.Offensive;
using SurvivorsGame.Items.Passive;
using SurvivorsGame.Systems;

namespace SurvivorsGame.UI.Menus;

public partial class LevelUpMenu : CanvasLayer
{
    [Export]
    private VBoxContainer _itemContainer;

    [Export]
    private uint _maxItems = 5;

    private Player Player => GameWorld.Instance.MainPlayer;
    private SceneTree Tree => GetTree();

    public override void _Ready()
    {
        PopulateItems();

        Input.MouseMode = Input.MouseModeEnum.Visible;
        Crosshair.Instance?.HideCrosshair();

        PauseController.Instance.Lock(this);
        PauseController.Instance.Pause(this);
    }

    public override void _ExitTree()
    {
        Crosshair.Instance?.ShowCrosshair();

        PauseController.Instance.Unlock(this);
        PauseController.Instance.Unpause(this);
    }

    private void PopulateItems()
    {
        for (var i = 0; i < _maxItems; i++)
        {
            BaseItem item;
            PackedScene scene;
            switch (i)
            {
                case 0:
                    scene = GD.Load<PackedScene>("uid://dyia7od5wyo4h");
                    item = scene.Instantiate<Pistol>();
                    break;
                case 1:
                    scene = GD.Load<PackedScene>("uid://fo0uohokdxdk");
                    item = scene.Instantiate<FlakJacket>();
                    break;
                case 2:
                    scene = GD.Load<PackedScene>("uid://covoknmwa82ef");
                    item = scene.Instantiate<TerroriserBeam>();
                    break;
                default:
                    return;
            }

            _itemContainer.GetChild<SelectableItem>(i).AssignItem(item);
        }
    }

    private void ItemSelected(BaseItem receivedItem)
    {
        switch (receivedItem)
        {
            case BaseOffensive offensive:
                Player?.OffensiveController.AddWeapon(offensive);
                break;

            case BasePassive passive:
                Player?.PassiveController.AddItem(passive);
                break;
        }

        QueueFree();
    }
}