namespace SurvivorsGame.UI.Menus;

public partial class Scroll : ScrollContainer
{
    /// <summary>
    ///     Used by SelectableItem scene AnimationPlayer to scroll since ScrollBar ratio property is hidden.
    /// </summary>
    [Export(PropertyHint.Range, "0,1,0.01")]
    public double ExposedRatio
    {
        get => GetHScrollBar().Ratio;
        set => GetHScrollBar().Ratio = value;
    }
}

