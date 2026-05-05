using Game.Levels.Controllers;
using Game.Players;

namespace Game.Interactables;

public partial class InteractableAirdropCrate : BaseInteractable
{
	[Export]
	private Node2D _interactableCircle = null!;

	[Export]
	private Sprite2D _interactableInnerCircle = null!;

	[Export]
	public int PickUpRange = 300;

	private bool _isMouseInsideArea;
	private bool _isCircleShowing;
	private bool _hasBeenInteractedWith;

	private Player Player => GameWorld.Instance.MainPlayer;
	private Vector2 PlayerPosition => Player.GlobalPosition;

	public override void _Process(double delta)
	{
		var distance = GlobalPosition.DistanceTo(PlayerPosition);
		if (distance <= PickUpRange)
		{
			_isMouseInsideArea = true;
			ShowInteractableCircle();
		}
		else
		{
			_isMouseInsideArea = false;
			HideInteractableCircle();
		}

		if (
			!_isMouseInsideArea
			|| !Input.IsActionPressed(InputMapNames.Interact)
		)
		{
			_interactableInnerCircle.Scale -= Vector2.One * (float)delta;
			_interactableInnerCircle.Scale =
				_interactableInnerCircle.Scale.Clamp(Vector2.Zero, Vector2.One);
			return;
		}

		_interactableInnerCircle.Scale += Vector2.One * (float)delta;
		_interactableInnerCircle.Scale = _interactableInnerCircle.Scale.Clamp(
			Vector2.Zero,
			Vector2.One
		);

		if (_interactableInnerCircle.Scale == Vector2.One)
			Interact();
	}

	private void ShowInteractableCircle()
	{
		if (_isCircleShowing)
			return;
		var tween = CreateTween().SetTrans(Tween.TransitionType.Expo);
		tween.TweenProperty(
			_interactableCircle,
			"modulate",
			Colors.White,
			0.5f
		);
		_isCircleShowing = true;
	}

	private void HideInteractableCircle()
	{
		if (!_isCircleShowing)
			return;
		var tween = CreateTween()
			.SetTrans(Tween.TransitionType.Expo)
			.SetParallel();
		tween.TweenProperty(
			_interactableCircle,
			"modulate",
			Colors.Transparent,
			0.5f
		);
		tween.TweenProperty(
			_interactableInnerCircle,
			"scale",
			Vector2.Zero,
			0.5f
		);
		_isCircleShowing = false;
	}

	public override void Interact()
	{
		if (_hasBeenInteractedWith)
			return;

		_hasBeenInteractedWith = true;

		EmitSignalOnInteraction();
		var tween = CreateTween()
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Expo);
		tween.TweenProperty(this, "modulate", Colors.Transparent, 0.5f);
		tween.TweenCallback(Callable.From(QueueFree));
	}
}
