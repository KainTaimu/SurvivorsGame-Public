using SurvivorsGame.Systems;

namespace SurvivorsGame.Pickups;

public abstract partial class BasePickup : RigidBody2D
{
    [Export]
    private float _attractionForce = 90f;

    private double _elapsedCooldownTime;

    private double _elapsedGiveUpTime;

    [Export]
    private double _giveUpCooldownSeconds = 0.9d;

    [Export]
    private double _giveUpTimeSeconds = 2d;

    protected int PickupValue;

    public override void _Ready()
    {
        GameWorld.Instance.AddPickup(this);
    }

    public override void _ExitTree()
    {
        GameWorld.Instance.RemovePickup(this);
    }

    public virtual int Pickup()
    {
        QueueFree();
        return PickupValue;
    }

    // BUG:
    // _elapsedGiveUpTime is never reset unless cooldown is hit.
    // Meaning attraction may stop randomly when attracting a previously attracted pickup.
    public void Attract(Vector2 position)
    {
        var delta = GetProcessDeltaTime();
        _elapsedGiveUpTime += delta;

        if (_elapsedGiveUpTime >= _giveUpTimeSeconds)
        {
            _elapsedCooldownTime += delta;
            if (_elapsedCooldownTime >= _giveUpCooldownSeconds)
            {
                _elapsedCooldownTime = 0d;
                _elapsedGiveUpTime = 0d;
            }

            return;
        }

        var direction = Position.DirectionTo(position) * _attractionForce * (float)delta * 1000;
        // ApplyImpulse(direction);
        ApplyForce(direction);
    }
}

