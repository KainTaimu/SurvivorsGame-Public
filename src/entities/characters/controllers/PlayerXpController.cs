using SurvivorsGame.Pickups;

namespace SurvivorsGame.Entities.Characters;

public partial class PlayerXpController : Area2D
{
    [Export]
    private CollisionShape2D _attractionShape2D;

    private CollisionShape2D _cachedCollisionShape;

    private float _collectionRadius;

    [Export]
    private CollisionShape2D _collectionShape2D;

    [Export]
    private PackedScene _levelUpMenuScene;

    [Export]
    private Player _owner;

    private PlayerStats _playerPlayerStats;

    public int Level { get; private set; } = 1;
    public int Xp { get; private set; }
    public int XpCap { get; private set; } = 10;

    public override void _Ready()
    {
        _playerPlayerStats = _owner.StatController.PlayerStats;
        UpdateAttractionArea();
    }

    public override void _Process(double delta)
    {
        if (!_owner.Alive)
        {
            return;
        }

        AttractXp();
    }

    public void GainXp(int xp)
    {
        var sumXp = xp * _playerPlayerStats.XpMultiplier;

        Xp += (int)Math.Ceiling(sumXp);

        if (Xp >= XpCap)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        Xp = 0;
        Level++;
        XpCap += (int)Math.Ceiling(Math.Pow(XpCap, 1.05f));
        CreateLevelUpMenu();
        _owner.EmitSignal(nameof(Player.PlayerLevelledUp));

        Logger.LogDebug($"Leveled up to {Level}. Xp cap at {XpCap}");
    }

    private void AttractXp()
    {
        foreach (var body in GetOverlappingBodies())
        {
            if (body is not BasePickup pickup)
            {
                return;
            }

            pickup.Attract(_owner.Position);

            if (_owner.Position.DistanceTo(pickup.Position) < _collectionRadius)
            {
                var xp = pickup.Pickup();
                GainXp(xp);
            }
        }
    }

    private void CreateLevelUpMenu()
    {
        var menu = _levelUpMenuScene.Instantiate();
        AddChild(menu);
    }

    private void UpdateAttractionArea()
    {
        if (_attractionShape2D.Shape is not CircleShape2D attractionCircle)
        {
            Logger.LogWarning("Updating attraction area failed due to its shape not being a CircleShape2D!");
            return;
        }

        if (_collectionShape2D.Shape is not CircleShape2D collectionCircle)
        {
            Logger.LogWarning("Updating collection area failed due to its shape not being a CircleShape2D!");
            return;
        }

        attractionCircle.Radius = _playerPlayerStats.PickupRange;

        _cachedCollisionShape = _owner.GetNode<CollisionShape2D>("HitArea");
        _collectionRadius = collectionCircle.Radius;
    }
}