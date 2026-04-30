using Game.Items.Offensive;
using Godot.Collections;

namespace Game.UI;

public partial class RevolverAmmoCount : CanvasLayer
{
	[Export]
	private Revolver _revolver = null!;

	[Export]
	private Sprite2D _revolverCylinderSprite = null!;

	[Export]
	private Array<Sprite2D> _cartidgeSprites = [];

	[Export]
	private Texture2D _firedCartridge = null!;

	[Export]
	private Texture2D _unfiredCartridge = null!;

	private float _cylinderRotation = 60f * (Mathf.Pi / 180);
	private const int _maxCylinderCount = 6;

	private int MagazineCapacity =>
		Math.Clamp(_revolver.MagazineCapacity, 0, _maxCylinderCount);

	public override void _Ready()
	{
		base._Ready();
		Callable
			.From(() =>
			{
				for (var i = 0; i < MagazineCapacity; i++)
				{
					Logger.LogDebug(i, MagazineCapacity);
					var sprite = _cartidgeSprites[i];
					sprite.Show();
				}
			})
			.CallDeferred();

		// BUG: When decreasing MagazineCapacity then increasing it again,
		// there will be a permanently visible cartridge because of
		// the conditionals in the for loops
		_revolver.OnReloadStart += () =>
		{
			// foreach (var sprite in _cartidgeSprites)
			for (var i = 0; i < MagazineCapacity; i++)
			{
				var sprite = _cartidgeSprites[i];
				sprite.Hide();
			}
			var tween = CreateTween()
				.SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Spring);
			_revolverCylinderSprite.Rotation = 0;
			tween.TweenProperty(
				_revolverCylinderSprite,
				"rotation",
				_revolverCylinderSprite.Rotation - _cylinderRotation * 6,
				_revolver.Stats.Additional["ReloadTimeMs"].AsSingle() / 1000 / 2
			);
		};
		_revolver.OnReloadEnd += () =>
		{
			for (var i = 0; i < MagazineCapacity; i++)
			{
				var sprite = _cartidgeSprites[i];
				sprite.Show();
				sprite.Texture = _unfiredCartridge;
			}
			var tween = CreateTween()
				.SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Spring);
			tween.TweenProperty(
				_revolverCylinderSprite,
				"rotation",
				0,
				_revolver.Stats.Additional["ReloadTimeMs"].AsSingle() / 1000 / 2
			);
		};
		_revolver.OnAttack += () =>
		{
			var origPos = _revolverCylinderSprite.Position;
			var tween = CreateTween().SetTrans(Tween.TransitionType.Spring);
			for (var i = 0; i < 6; i++)
			{
				static int rand() => GD.RandRange(-1, 1);
				var shake = new Vector2(rand(), rand()) * GD.RandRange(4, 9);

				tween.TweenProperty(
					_revolverCylinderSprite,
					"position",
					_revolverCylinderSprite.Position + shake,
					1 / 30f
				);
			}
			tween.TweenProperty(
				_revolverCylinderSprite,
				"position",
				origPos,
				1 / 8f
			);
			_cartidgeSprites[
				MagazineCapacity - _revolver.MagazineCount - 1
			].Texture = _firedCartridge;
		};
	}

	// HACK:
	// Because of inheritance fuckery, cannot set _revolver as Node2D then
	// connect to visibility_changed signal to hide this CanvasLayer normally.
	public override void _Notification(int what)
	{
		if (what == NotificationPaused)
			Hide();
		else if (what == NotificationUnpaused)
			Show();
	}

	public void RotateCylinder()
	{
		var nextRotation = _revolverCylinderSprite.Rotation - _cylinderRotation;

		var tween = CreateTween();
		tween.SetEase(Tween.EaseType.Out);
		tween.SetTrans(Tween.TransitionType.Spring);
		tween.TweenProperty(
			_revolverCylinderSprite,
			"rotation",
			nextRotation,
			0.3f
		);
	}
}
