using Game.UI;

namespace Game.Players.Controllers;

public partial class PlayerMovementController : Node2D
{
	[Export]
	public float ExhaustionTime = 1.5f;

	[Export]
	public bool NoClip;

	[Export]
	private Player _player = null!;

	[Export]
	private AnimatedSprite2D _sprite = null!;

	public Vector2 Velocity;

	private Rid _navigationMap;

	private Viewport? Viewport => GetViewport();
	private Crosshair? Crosshair => Crosshair.Instance;
	private CharacterStats CharacterStats => _player.Character.CharacterStats;

	private bool _isSprinting;
	private float _exhaustion;

	public override void _Ready()
	{
		Callable.From(() => Crosshair?.OnCrosshairMoved += FlipSprite).CallDeferred();
		_navigationMap = GetWorld2D().NavigationMap;
	}

	public override void _PhysicsProcess(double delta)
	{
		Velocity = Vector2.Zero;
		PlayerMovement(delta);
		if (!_isSprinting)
		{
			if (_exhaustion > 0)
			{
				_exhaustion = Mathf.Max(0, _exhaustion - (float)delta);
				return;
			}
			CharacterStats.Stamina += (float)delta;
		}
	}

	private void PlayerMovement(double delta)
	{
		var up = Input.IsActionPressed(InputMapNames.MoveUp) ? 1 : 0;
		var down = Input.IsActionPressed(InputMapNames.MoveDown) ? 1 : 0;
		var left = Input.IsActionPressed(InputMapNames.MoveLeft) ? 1 : 0;
		var right = Input.IsActionPressed(InputMapNames.MoveRight) ? 1 : 0;

		if (up + down + left + right <= 0)
			return;

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
			_sprite.Animation = "idle";

		var move =
			new Vector2(inputX * CharacterStats.MoveSpeed, inputY * CharacterStats.MoveSpeed)
			* CharacterStats.MoveSpeedMultiplier;

		_isSprinting = Input.IsKeyLabelPressed(Key.Shift);

		if (_isSprinting && CharacterStats.Stamina > 0)
		{
			move *= Mathf.Max(1, CharacterStats.RunSpeed * CharacterStats.RunSpeedMultiplier);
			CharacterStats.Stamina -= (float)delta;
			_exhaustion = ExhaustionTime;
		}

		Velocity = move;
		move *= (float)delta;
		var originalPos = _player.GetPosition();

		var newPos = originalPos + move;

		const float dist = 5;
		// NOTE: Could shape cast with World2D.DirectSpaceState, but using navigation map
		// also prevents the player from cheesing by hiding in a place the navmesh doesnt cover
		var closest = NavigationServer2D.MapGetClosestPoint(_navigationMap, newPos);
		if (Position.DistanceSquaredTo(closest) > dist * dist && !NoClip)
			_player.GlobalPosition = closest;
		else
			_player.GlobalPosition = newPos;
	}

	private void FlipSprite()
	{
		if (Crosshair is null || Viewport is null)
			return;

		var mouse = Crosshair.CanvasSpacePosition / Viewport.GetVisibleRect().Size;
		_sprite.FlipH = mouse.X < 0.5;
	}
}
