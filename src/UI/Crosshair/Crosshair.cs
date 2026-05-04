using Game.Players;
using Game.UI.Menus;

namespace Game.UI;

public partial class Crosshair : Node2D
{
	[Signal]
	public delegate void OnCrosshairMovedEventHandler();

	[Export]
	public Player Player = null!;

	[Export]
	public AnimatedSprite2D PrimaryCrosshairSprite { get; private set; } =
		null!;

	[Export]
	public AnimatedSprite2D SecondaryCrosshairSprite { get; private set; } =
		null!;

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

	public float PrimaryCrosshairSpreadRatio =>
		PrimaryCrosshairSprite.Frame
		/ PrimaryCrosshairSprite.SpriteFrames.GetFrameCount(
			PrimaryCrosshairSprite.Animation
		);
	public float SecondaryCrosshairSpreadRatio =>
		SecondaryCrosshairSprite.Frame
		/ SecondaryCrosshairSprite.SpriteFrames.GetFrameCount(
			SecondaryCrosshairSprite.Animation
		);

	public Vector2 CanvasSpacePosition =>
		PrimaryCrosshairSprite.GetCanvasTransform()
		* PrimaryCrosshairSprite.GlobalPosition;

	public Vector2 GlobalSpacePosition =>
		Viewport.CanvasTransform.AffineInverse()
		* PrimaryCrosshairSprite.GlobalPosition;

	public static Crosshair? Instance { get; private set; }

	public float AngleFromPlayer => GetAngleFromPlayer();
	public Vector2 CrosshairVelocity;

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

	public override void _Process(double delta)
	{
		PrimaryCrosshairSprite.Frame = 0;
		SecondaryCrosshairSprite.Frame = 0;
		Callable.From(() => CrosshairVelocity = Vector2.Zero).CallDeferred();
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
		CrosshairVelocity += motion.Relative;
		ClampCrosshairToViewport();
		EmitSignalOnCrosshairMoved();
	}

	public void ChangePrimaryCrosshairSpread(float spreadRatio)
	{
		if (spreadRatio < 0 || spreadRatio > 1)
			Logger.LogError("spreadRatio should be 0 < spreadRatio < 1");
		var frameCount = PrimaryCrosshairSprite.SpriteFrames.GetFrameCount(
			PrimaryCrosshairSprite.Animation
		);
		var idx = (int)Math.Round(spreadRatio * frameCount);
		PrimaryCrosshairSprite.Frame = Mathf.Clamp(idx, 0, frameCount);
	}

	public void ChangeSecondaryCrosshairSpread(float spreadRatio)
	{
		if (spreadRatio < 0 || spreadRatio > 1)
			Logger.LogError("spreadRatio should be 0 < spreadRatio < 1");
		var frameCount = SecondaryCrosshairSprite.SpriteFrames.GetFrameCount(
			SecondaryCrosshairSprite.Animation
		);
		var idx = (int)Math.Round(spreadRatio * frameCount);
		SecondaryCrosshairSprite.Frame = Mathf.Clamp(idx, 0, frameCount);
	}

	public void ChangeCrosshairSize(float newSize)
	{
		PrimaryCrosshairSprite.Scale = new Vector2(1, 1) * newSize;
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
			PrimaryCrosshairSprite.GlobalPosition
			* Viewport.GetScreenTransform();

		return playerPos.AngleToPoint(crosshairPos);
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
		PrimaryCrosshairSprite.GlobalPosition =
			PrimaryCrosshairSprite.GlobalPosition.Clamp(min, max);
	}

	public partial class CrossHairRecoil(Crosshair crosshair) : Node
	{
		private Vector2 _accumilatedImpulse = Vector2.Zero;
		private float _impulseScale = 1;
		private Tween? _impulseTweener;
		private Tween? _recoilJumpTweener;

		public void ApplyImpulse(
			Vector2 impulse,
			float accumilatedImpuseFactor = 1f
		)
		{
			const float easeReturn = 0.2f;

			var targetCrosshair = crosshair.PrimaryCrosshairSprite;

			_accumilatedImpulse += impulse * accumilatedImpuseFactor;
			_impulseScale += 0.1f;
			_impulseScale = Math.Clamp(_impulseScale, 0f, 1f);

			// Punish avoiding vertical recoil by shooting above or below center
			var crosshairScreenPosRatio =
				crosshair.CanvasSpacePosition
				/ crosshair.GetViewport().GetVisibleRect().Size;
			if (
				(
					crosshairScreenPosRatio.Y < 0.3
					|| crosshairScreenPosRatio.Y > 0.7
				)
				&& crosshairScreenPosRatio.X > 0.3
				&& crosshairScreenPosRatio.X < 0.7
			)
			{
				// account for larger horizontal size on 16:9 screens
				impulse = new Vector2(
					impulse.Y
						* crosshair.GetViewport().GetVisibleRect().Size.X
						* 0.001111111f // Arbitrary scaling factor for X
						* (
							GD.Randf() < 0.5
								? (float)GD.RandRange(-1, -0.5)
								: (float)GD.RandRange(0.5, 1)
						),
					impulse.X * 0.8f
				);
			}

			var finalImpulseVector =
				impulse + (_accumilatedImpulse * _impulseScale);
			var finalCrosshairPos =
				targetCrosshair.Position + finalImpulseVector;

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
