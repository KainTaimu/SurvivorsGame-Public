using Game.Players;
using Game.UI.Menus;

namespace Game.UI;

public partial class Crosshair : Node2D
{
	[Export]
	public Player Player = null!;

	[Export]
	public AnimatedSprite2D CrosshairSprite { get; private set; } = null!;

	[Export]
	public PauseController PauseController { get; private set; } = null!;

	[Export(PropertyHint.Range, "0,5,0.25")]
	private float CrosshairSize
	{
		get;
		set
		{
			field = value;
			Callable.From(() => ChangeCrosshairSize(value)).CallDeferred();
		}
	} = 1;

	public static Crosshair? Instance { get; private set; }

	public float AngleFromPlayer => GetAngleFromPlayer();

	public CrossHairRecoil Recoil { get; private set; } = null!;

	private Input.MouseModeEnum _hiddenMouseMode = Input.MouseModeEnum.Visible;
	private Input.MouseModeEnum _visibleMouseMode = Input
		.MouseModeEnum
		.Captured;

	private Viewport Viewport => GetViewport();

	public override void _Ready()
	{
		Instance = this;

		Recoil = new CrossHairRecoil(this);

		Input.SetMouseMode(_visibleMouseMode);
		Position = GetViewportRect().GetCenter();

		PauseController.OnPause += HideCrosshair;
		PauseController.OnUnpause += ShowCrosshair;

		ChangeCrosshairSize(CrosshairSize);
	}

	public override void _ExitTree()
	{
		Input.SetMouseMode(_hiddenMouseMode);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is not InputEventMouseMotion motion)
			return;
		Position += motion.Relative;
		ClampCrosshairToViewport();
	}

	public void ChangeCrosshairSize(float newSize)
	{
		CrosshairSprite.Scale = new Vector2(1, 1) * newSize;
	}

	public void ShowCrosshair()
	{
		Show();
		Input.SetMouseMode(_visibleMouseMode);
	}

	public void HideCrosshair()
	{
		Hide();
		Input.SetMouseMode(_hiddenMouseMode);
	}

	private float GetAngleFromPlayer()
	{
		var playerPos =
			Player.GlobalPosition
			* Viewport.GetCamera2D().GetCanvasTransform().AffineInverse();
		var crosshairPos =
			CrosshairSprite.GlobalPosition * Viewport.GetScreenTransform();

		var angle = playerPos.AngleToPoint(crosshairPos);
		return angle;
	}

	private void ClampCrosshairToViewport()
	{
		const int marginPx = 0;
		var viewportSize = Viewport.GetVisibleRect().Size;
		var min = Vector2.One * -marginPx;
		var max = new Vector2(
			viewportSize.X + marginPx,
			viewportSize.Y + marginPx
		);
		CrosshairSprite.GlobalPosition = CrosshairSprite.GlobalPosition.Clamp(
			min,
			max
		);
	}

	public partial class CrossHairRecoil(Crosshair crosshair) : Node
	{
		/// Ideas:
		///     - Bounce recoil: Crosshair jumps per shot
		///         - Rotation so horizontal recoil is tangent to mouse relative to player?
		///     - Bloom recoil: Accurate until nth shot, where accuracy exponentially decreases per shot
		private Vector2 _accumilatedImpulse = Vector2.Zero;
		private float _impulseScale = 1;
		private Tween? _impulseTweener;
		private Tween? _recoilJumpTweener;

		public void ApplyImpulse(Vector2 impulse, float minImpulse = 0.6f)
		{
			const float easeReturn = 0.2f;

			var targetCrosshair = crosshair.CrosshairSprite;

			_accumilatedImpulse += impulse;
			_impulseScale += 0.1f;
			_impulseScale = Math.Clamp(_impulseScale, minImpulse, 1f);

			var finalCrosshairPos =
				targetCrosshair.Position
				+ (_accumilatedImpulse * _impulseScale);

			_recoilJumpTweener?.Kill();
			_recoilJumpTweener = crosshair
				.CreateTween()
				.SetTrans(Tween.TransitionType.Elastic)
				.SetEase(Tween.EaseType.Out);
			_recoilJumpTweener.TweenProperty(
				targetCrosshair,
				"position",
				finalCrosshairPos,
				0.33f
			);

			_impulseTweener?.Kill();
			_impulseTweener = crosshair
				.CreateTween()
				.SetTrans(Tween.TransitionType.Linear);
			_impulseTweener.TweenProperty(
				this,
				"_accumilatedImpulse",
				Vector2.Zero,
				easeReturn
			);
			_impulseTweener.TweenProperty(this, "_impulseScale", 0f, 0.6f);

			crosshair.ClampCrosshairToViewport();
		}
	}
}
