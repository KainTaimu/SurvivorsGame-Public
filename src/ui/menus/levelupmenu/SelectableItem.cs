using SurvivorsGame.Items;
using SurvivorsGame.Items.Offensive;
using SurvivorsGame.Items.Passive;

namespace SurvivorsGame.UI.Menus;

public partial class SelectableItem : PanelContainer
{
    [Signal]
    public delegate void LevelUpPressedEventHandler(BaseItem item);

    private BaseItem _assignedItem;

    private HScrollBar _containerScrollBar;

    [Export]
    private Label _descriptionLabel;

    [Export]
    private HSplitContainer _hSplitContainer;

    private bool _isHighlighted;

    [Export]
    private Label _nameLabel;

    [Export]
    private PanelContainer _rightPanel;

    [Export]
    private AnimationPlayer _scrollAnimator;

    [Export]
    private ScrollContainer _scrollContainer;

    private Tween _tween;

    public override void _Ready()
    {
        _containerScrollBar = _scrollContainer.GetHScrollBar();
        _rightPanel.GuiInput += RightPanelOnGuiInput;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex != MouseButton.Left)
            {
                return;
            }

            if (!mouseButton.IsReleased())
            {
                return;
            }

            OnClick();
        }
    }

    private void RightPanelOnGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouse mouse)
        {
            HoverScroll(mouse.GetPosition());
        }
    }

    public void AssignItem(BaseItem item)
    {
        if (_assignedItem is not null)
        {
            Logger.LogWarning("An attempt to re-assign an item was made.");
            return;
        }

        switch (item)
        {
            case BaseOffensive offensive:
                _nameLabel.Text = offensive.Properties.Name;
                _descriptionLabel.Text = offensive.Properties.Description;
                break;

            case BasePassive passive:
                _nameLabel.Text = passive.Properties.Name;
                _descriptionLabel.Text = passive.Properties.Description;
                break;

            default:
                return;
        }

        _assignedItem = item;
        _hSplitContainer.Show();
    }

    private void Highlight(bool flag)
    {
        if (!_hSplitContainer.IsVisible())
        {
            return;
        }

        _containerScrollBar.Ratio = 0;
        if (flag)
        {
            SelfModulate = new Color(1, 1, 1);
            _isHighlighted = true;
            _scrollAnimator.Stop();
        }
        else
        {
            SelfModulate = new Color(0, 0, 0, 0);
            _isHighlighted = false;
            _scrollAnimator.Play("scroll");
        }
    }

    private void HoverScroll(Vector2 mousePos)
    {
        const float leftSnap = 0.03f;
        const float rightSnap = 0.97f;
        var ratio = mousePos.X / _rightPanel.Size.X;

        if (ratio < leftSnap)
        {
            _containerScrollBar.Ratio = 0;
            return;
        }

        if (ratio > rightSnap)
        {
            _containerScrollBar.Ratio = 1;
            return;
        }

        _containerScrollBar.Ratio = ratio;
    }

    private void OnClick()
    {
        if (!_hSplitContainer.IsVisible())
        {
            return;
        }

        EmitSignal(nameof(LevelUpPressed), _assignedItem);
    }
}

