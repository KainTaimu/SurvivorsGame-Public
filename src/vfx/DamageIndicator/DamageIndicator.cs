using System.Globalization;

namespace SurvivorsGame.VFX;

public partial class DamageIndicator : Node2D
{
    private static int _indicatorCount;

    private Vector2 _finalPosition;

    [Export]
    private Label _label;

    private Tween _tween;

    public override void _Ready()
    {
        if (_indicatorCount > 100)
        {
            Logger.LogDebug("Too many indicators! " + _indicatorCount);
            QueueFree();
            return;
        }

        _indicatorCount++;
    }

    public override void _ExitTree()
    {
        _indicatorCount--;
    }

    public void ShowIndicator(Node2D obj, int damage)
    {
        const float variation = 10;
        var randomOffset1 = (float)GD.RandRange(-variation, variation);
        var randomOffset2 = (float)GD.RandRange(-variation, variation);

        Position = new Vector2(obj.Position.X + randomOffset1, obj.Position.Y + randomOffset2 - 55);
        _label.Text = damage.ToString(CultureInfo.InvariantCulture);
        _finalPosition = new Vector2(Position.X + 15, Position.Y - 15);

        _tween?.Kill();
        _tween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        _tween.Parallel().TweenProperty(this, "position", _finalPosition, 1.75f);
        _tween.Parallel().TweenProperty(this, "modulate", new Color(1, 1, 1, 0), 1.75f);
        _tween.TweenCallback(Callable.From(QueueFree));
    }
}