using Godot.Collections;
using SurvivorsGame.Entities.Enemies;

namespace SurvivorsGame.Items.Projectiles;

public partial class LaserBeam : BaseProjectile
{
    [Export] private Line2D _beamSprite;
    [Export] private Curve _beamWidthCurve = new();
    private double _lifetime;
    private Array<Rid> _previousHits = []; // Keep Rids to prevent multiple hits in one frame

    public override void _Ready()
    {
        _lifetime = ProjectileSpeed;
        var points = _beamSprite.GetPoints();
        if (points.Length != 2)
        {
            Logger.LogError($"Expected 2 points in Line2D, got {points.Length}");
            QueueFree();
            return;
        }

        while (true)
        {
            if (CastRay(out var result))
            {
                var area2D = (Area2D)result["collider"];
                var parent = area2D.GetParent();
                if (parent is BaseEnemy enemy)
                {
                    OnEnemyContact(enemy);
                }

                _previousHits.Add((Rid)result["rid"]);
            }
            else
            {
                return;
            }
        }
    }

    public override void _Process(double delta)
    {
        _lifetime -= delta;
        var normalizedX = Mathf.Clamp((float)(_lifetime / ProjectileSpeed), 0f, 1f);
        var beamWidth = _beamWidthCurve.Sample(normalizedX);
        _beamSprite.SetWidth(Mathf.Max(0f, beamWidth));

        if (_lifetime <= 0)
        {
            QueueFree();
        }
    }

    protected override void OnEnemyContact(BaseEnemy enemy)
    {
        HandleHitEnemy(enemy);
    }

    // See Issue #1
    protected override void HandleHitEnemy(BaseEnemy enemy)
    {
        EmitSignal(nameof(HitEnemy), enemy);
    }

    private bool CastRay(out Dictionary result)
    {
        var spaceState = GetWorld2D().DirectSpaceState;
        var origin = GlobalPosition;
        var direction = Vector2.Right.Rotated(GlobalRotation);
        var end = origin + direction * 9999;

        var query = PhysicsRayQueryParameters2D.Create(origin, end);
        query.CollideWithAreas = true;
        query.SetCollisionMask(0x200); // Layer 10 "EnemyHitbox"
        query.Exclude = _previousHits;

        result = spaceState.IntersectRay(query);

        if (result.Count <= 0)
        {
            return false;
        }

        return true;
    }
}