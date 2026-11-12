using Game.UI;

namespace Game.VFX;

public partial class ScopeComponent : CanvasLayer
{
	[Signal]
	public delegate void OnScopedInEventHandler();

	[Signal]
	public delegate void OnScopedOutEventHandler();

	[Export]
	private float _zoomInTime = 0.4f;

	[Export]
	private float MaxRadius
	{
		get;
		set
		{
			field = value;
			_overlay.SetInstanceShaderParameter(_radiusName, value);
		}
	} = 0.16f;

	[Export]
	private float Feather
	{
		get;
		set
		{
			field = value;
			_overlay.SetInstanceShaderParameter(_featherName, value);
		}
	} = 0.054f;

	[Export]
	private Control _overlay = null!;

	public bool ScopedIn => _toggled;

	private Tween? _tween;
	private bool _toggled;

	private static readonly StringName _pointName = "point";
	private static readonly StringName _radiusName = "radius";
	private static readonly StringName _featherName = "feather";

	public override void _Ready()
	{
		_toggled = false;
		Hide();
	}

	public void Process()
	{
		if (Input.IsActionJustPressed(InputMapNames.SecondaryAttack))
		{
			_toggled = !_toggled;
			if (_toggled)
				ShowScope();
			else
				HideScope();
		}

		UpdateScope();
	}

	public void UpdateScope()
	{
		var cs = Crosshair.Instance;
		if (cs is null)
			return;
		var vp = GetViewport().GetVisibleRect();
		var pos = cs.CanvasSpacePosition;
		var norm = pos with { X = pos.X / vp.Size.X, Y = pos.Y / vp.Size.Y };

		_overlay.SetInstanceShaderParameter(_pointName, norm);
	}

	public void HideScope()
	{
		_toggled = false;
		_overlay.SetInstanceShaderParameter(_radiusName, MaxRadius);
		_tween?.Kill();
		_tween = CreateTween().SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		_tween.TweenMethod(
			Callable.From((float i) => _overlay.SetInstanceShaderParameter(_radiusName, i)),
			MaxRadius,
			1.5f,
			_zoomInTime
		);
		_tween.TweenCallback(Callable.From(Hide));
		EmitSignalOnScopedOut();
	}

	public void ShowScope()
	{
		_toggled = true;
		_overlay.SetInstanceShaderParameter(_radiusName, 1f);
		_tween?.Kill();
		_tween = CreateTween().SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		_tween.TweenMethod(
			Callable.From((float i) => _overlay.SetInstanceShaderParameter(_radiusName, i)),
			1.5f,
			MaxRadius,
			_zoomInTime
		);
		Show();
		EmitSignalOnScopedIn();
	}
}
