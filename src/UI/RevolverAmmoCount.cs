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
	private Array<Sprite2D> _cartidges = [];

	private float _cylinderRotation = 60f * (Mathf.Pi / 180);

	public override void _Ready()
	{
		base._Ready();
		_revolver.OnReloadStart += () =>
		{
			foreach (var sprite in _cartidges)
				sprite.Hide();
			var tween = CreateTween()
				.SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Spring);
			_revolverCylinderSprite.Rotation = 0;
			tween.TweenProperty(
				_revolverCylinderSprite,
				"rotation",
				_revolverCylinderSprite.Rotation + _cylinderRotation * 6,
				_revolver.Stats.Additional["ReloadTimeMs"].AsSingle() / 1000 / 2
			);
		};
		_revolver.OnReloadEnd += () =>
		{
			foreach (var sprite in _cartidges)
				sprite.Show();
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
		};
	}

	public void RotateCylinder()
	{
		foreach (var sprite in _cartidges)
		{
			sprite.Hide();
		}
		for (var i = 0; i < _revolver.MagazineCount; i++)
			_cartidges[5 - i % 6].Show();

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
