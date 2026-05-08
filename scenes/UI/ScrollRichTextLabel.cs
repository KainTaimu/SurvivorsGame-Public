namespace Game.UI;

public partial class ScrollRichTextLabel : ScrollContainer
{
    [Export(PropertyHint.Range, "0,1,0.01")]
    public double VScrollBarRatio
    {
        get => GetVScrollBar().Ratio;
        set => GetVScrollBar().Ratio = value;
    }

    [Export]
    public AnimationPlayer ScrollAnimationPlayer = null!;

    private double _maxRatio;

    [Export]
    private PanelContainer _panelParent = null!;

    [Export]
    private Label _label = null!;

    public override void _Ready()
    {
        // MouseEntered += () => ScrollAnimationPlayer.Play("scroll");
        // MouseExited += () => ScrollAnimationPlayer.Stop();
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is not InputEventMouse mouse)
            return;

        // HoverScroll(mouse.GetPosition());
    }

    private void HoverScroll(Vector2 mousePos)
    {
        // const float leftSnap = 0.03f;
        // const float rightSnap = 0.97f;

        var ratio = mousePos.Y / Size.Y;

        // if (ratio < leftSnap)
        // {
        // 	VScrollBarRatio = 0;
        // 	return;
        // }
        //
        // if (ratio > rightSnap)
        // {
        // 	VScrollBarRatio = 1;
        // 	return;
        // }

        // ScrollVertical = (int)mousePos.Y;
        SetDeferred(PropertyName.ScrollVertical, _label.Size.Y * ratio);
        Logger.LogDebug(ScrollVertical, Size.Y);
    }
}
