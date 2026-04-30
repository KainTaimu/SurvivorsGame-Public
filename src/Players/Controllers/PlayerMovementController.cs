using Game.UI;

namespace Game.Players.Controllers;

public partial class PlayerMovementController : Node
{
	[Export]
	private Player _player = null!;

	private CharacterStats CharacterStats => _player.Character.CharacterStats;

	[Export]
	private AnimatedSprite2D _sprite = null!;

	private Viewport? Viewport => GetViewport();
	private Crosshair? Crosshair => Crosshair.Instance;

	public override void _Ready()
	{
		Callable
			.From(() => Crosshair?.OnCrosshairMoved += FlipSprite)
			.CallDeferred();
	}

	public override void _Process(double delta)
	{
		PlayerMovement(delta);
	}

	private void PlayerMovement(double delta)
	{
		var up = Input.IsActionPressed(InputMapNames.MoveUp) ? 1 : 0;
		var down = Input.IsActionPressed(InputMapNames.MoveDown) ? 1 : 0;
		var left = Input.IsActionPressed(InputMapNames.MoveLeft) ? 1 : 0;
		var right = Input.IsActionPressed(InputMapNames.MoveRight) ? 1 : 0;

		float inputX = right - left;
		float inputY = down - up;

		var moveLength = (float)Math.Sqrt(inputX * inputX + inputY * inputY);

		if (moveLength > 0)
		{
			inputX /= moveLength;
			inputY /= moveLength;
			_sprite.Animation = "run";
		}
		else
		{
			_sprite.Animation = "idle";
		}

		var move = new Vector2(
			inputX
				* (
					CharacterStats.MoveSpeed
					* CharacterStats.MoveSpeedMultiplier
				),
			inputY
				* (
					CharacterStats.MoveSpeed
					* CharacterStats.MoveSpeedMultiplier
				)
		);
		move *= (float)delta;
		var originalPos = _player.GetPosition();

		var newPos = originalPos + move;
		_player.SetPosition(newPos);
	}

	private void FlipSprite()
	{
		if (Crosshair is null || Viewport is null)
			return;

		var mouse =
			Crosshair.CanvasSpacePosition / Viewport.GetVisibleRect().Size;
		_sprite.FlipH = mouse.X < 0.5;
	}
}
