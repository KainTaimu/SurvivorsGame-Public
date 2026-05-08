namespace Game.VFX;

// NOTE: Tightly coupled with DamageIndicatorPool
public partial class DamageIndicator : Node2D
{
	[Signal]
	public delegate void OnFinishedEventHandler(DamageIndicator indicator);

	private Vector2 _finalPosition;

	[Export]
	private Label _label = null!;

	private Tween? _tween;

	public void ShowIndicator(Vector2 pos, int damage, bool isCrit = false)
	{
		Show();
		const float variation = 10;
		var randomOffsetX = (float)GD.RandRange(-variation, variation);
		var randomOffsetY = (float)GD.RandRange(-variation, variation);

		Position = new Vector2(
			pos.X + randomOffsetX,
			pos.Y + randomOffsetY - 55
		);
		_label.Text = damage.ToString();
		_label.LabelSettings.FontColor = isCrit
			? Colors.OrangeRed
			: Colors.White;

		_finalPosition = new Vector2(Position.X + 15, Position.Y - 15);

		_tween?.Kill();
		_tween = CreateTween()
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.Out);
		_tween
			.Parallel()
			.TweenProperty(this, "position", _finalPosition, 1.75f);
		_tween
			.Parallel()
			.TweenProperty(this, "modulate", Colors.Transparent, 1.75f);
		_tween.TweenCallback(
			Callable.From(() =>
			{
				Reset();
				EmitSignalOnFinished(this);
			})
		);
	}

	public void Reset()
	{
		_tween?.Kill();
		_tween = null;
		Hide();
		Modulate = Colors.White;
	}
}
