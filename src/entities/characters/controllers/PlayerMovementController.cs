using SurvivorsGame.Systems;

namespace SurvivorsGame.Entities.Characters;

public partial class PlayerMovementController : Node
{
    [Export]
    private Player _owner;

    public float Facing { get; private set; }

    public override void _Ready()
    {
        _owner.Sprite.Play();
    }

    public override void _Process(double delta)
    {
        UpdateFacingDirection();
        PlayerMovement(delta);
    }

    public override void _Input(InputEvent @event)
    {
        switch (@event)
        {
            case InputEventMouseMotion:
                FlipSprite();
                break;
        }
    }

    private void PlayerMovement(double delta)
    {
        var right = Convert.ToInt32(Input.IsActionPressed("move_right"));
        var left = Convert.ToInt32(Input.IsActionPressed("move_left"));
        var up = Convert.ToInt32(Input.IsActionPressed("move_up"));
        var down = Convert.ToInt32(Input.IsActionPressed("move_down"));

        float inputX = right - left;
        float inputY = down - up;

        var moveLength = (float)Math.Sqrt(inputX * inputX + inputY * inputY);

        if (moveLength > 0)
        {
            inputX /= moveLength;
            inputY /= moveLength;
            _owner.Sprite.Animation = "run";
        }
        else
        {
            _owner.Sprite.Animation = "idle";
        }

        var move = new Vector2(
            inputX * (_owner.StatController.PlayerStats.MoveSpeed *
                      _owner.StatController.PlayerStats.MoveSpeedMultiplier),
            inputY * (_owner.StatController.PlayerStats.MoveSpeed *
                      _owner.StatController.PlayerStats.MoveSpeedMultiplier));
        move *= (float)delta;
        var originalPos = _owner.GetPosition();

        var newPos = originalPos + move;
        newPos = newPos.Clamp(Vector2.Zero,
            new Vector2(GameWorld.Instance.CurrentLevel.PixelSize.X, GameWorld.Instance.CurrentLevel.PixelSize.Y));

        _owner.SetPosition(newPos);
    }

    private void UpdateFacingDirection()
    {
        var playerVector = _owner.GetCanvasTransform() * _owner.Position;
        var mouseVector = GetViewport().GetMousePosition();

        Facing = playerVector.AngleToPoint(mouseVector);
    }

    private void FlipSprite()
    {
        if ((Facing <= MathF.PI / 2) & (Facing > -(MathF.PI / 2)))
        {
            _owner.Sprite.FlipH = false;
        }
        else
        {
            _owner.Sprite.FlipH = true;
        }
    }
}