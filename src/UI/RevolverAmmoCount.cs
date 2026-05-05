using Game.Items.Offensive;
using Game.Levels.Controllers;
using Game.Players.Controllers;
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

	[Export]
	private Marker2D _leftPosition = null!;

	[Export]
	private Marker2D _rightPosition = null!;

	private float _cylinderRotation = 60f * (Mathf.Pi / 180);
	private const int _maxCylinderCount = 6;
	private Vector2 _originalPosition;

	private PlayerWeaponController WeaponController =>
		GameWorld.Instance.MainPlayer.WeaponController;
	private int MagazineCapacity =>
		Math.Clamp(_revolver.MagazineCapacity, 0, _maxCylinderCount);

	public override void _Ready()
	{
		base._Ready();

		// Properties may not be properly set by this point.
		// Wait till next frame
		Callable
			.From(() =>
			{
				for (var i = 0; i < MagazineCapacity; i++)
				{
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
				_revolver.FirearmStats?.ReloadTimeMs / 1000 / 2 ?? 0
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
				_revolver.FirearmStats?.ReloadTimeMs / 1000 / 2 ?? 0
			);
		};
		_revolver.OnAttack += () =>
		{
			var tween = CreateTween().SetTrans(Tween.TransitionType.Spring);
			// Cylinder Shake
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
				_originalPosition,
				1 / 8f
			);
			_cartidgeSprites[
				MagazineCapacity - _revolver.MagazineCount - 1
			].Texture = _firedCartridge;
		};
		WeaponController.OnPrimaryAttackReassigned += () =>
			Callable.From(SwapCylinderSide).CallDeferred();
		WeaponController.OnSecondaryAttackReassigned += () =>
			Callable.From(SwapCylinderSide).CallDeferred();
	}

	private void SwapCylinderSide()
	{
		if (_revolver.AttackActionString is null)
		{
			Hide();
			return;
		}
		else
			Show();

		var useLeft =
			_revolver.AttackActionString == InputMapNames.PrimaryAttack;
		if (useLeft)
			_revolverCylinderSprite.Reparent(_leftPosition, false);
		else
			_revolverCylinderSprite.Reparent(_rightPosition, false);

		// _originalPosition = _revolverCylinderSprite.Position;
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
