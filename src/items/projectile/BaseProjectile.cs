using SurvivorsGame.Entities.Enemies;
using SurvivorsGame.Items.Offensive;

namespace SurvivorsGame.Items.Projectiles;

public partial class BaseProjectile : Area2D
{
    [Signal]
    public delegate void HitEnemyEventHandler(BaseEnemy enemy);

    protected bool HasHitThisFrame;

    protected int PierceCount;

    public int PierceLimit;

    public float ProjectileSpeed;

    public BaseOffensive WeaponOrigin;

    public Sprite2D Sprite { get; private set; }

    public override void _Process(double delta)
    {
        HasHitThisFrame = false;
        MoveTowardPoint(delta);
    }

    private void OnAreaEntered(Area2D area)
    {
        switch (area)
        {
            case BotHitbox enemy:
                OnEnemyContact(enemy.EnemyOwner);
                break;
        }
    }

    protected virtual void OnEnemyContact(BaseEnemy enemy)
    {
        HandleHitEnemy(enemy);
    }

    // See Issue #1
    protected virtual void HandleHitEnemy(BaseEnemy enemy)
    {
        if (HasHitThisFrame)
        {
            return;
        }

        PierceCount++;
        if (PierceLimit != -1 && PierceCount >= PierceLimit)
        {
            QueueFree();
        }

        EmitSignal(nameof(HitEnemy), enemy);
        HasHitThisFrame = true;
    }

    private void MoveTowardPoint(double delta)
    {
        var moveVector = new Vector2(1, 0).Rotated(Rotation) * ProjectileSpeed * (float)delta;

        Position += moveVector;
    }

    private void OnVisibleOnScreenNotifier2DScreenExited()
    {
        QueueFree();
    }
}