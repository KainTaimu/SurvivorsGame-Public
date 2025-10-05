using SurvivorsGame.Entities.Enemies;

namespace SurvivorsGame.Items.Projectiles;

public partial class ProjectileRaygun : BaseProjectile
{
    [Export]
    private Area2D _areaOfEffectArea;

    public override void _Ready()
    {
        base._Ready();
        var tweenSpeed = CreateTween().BindNode(this).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
        var originalSpeed = ProjectileSpeed;

        ProjectileSpeed = originalSpeed * 0.5f;
        tweenSpeed.TweenProperty(this, nameof(ProjectileSpeed), originalSpeed, .75f);
    }

    protected override void OnEnemyContact(BaseEnemy enemy)
    {
        if (HasHitThisFrame)
        {
            return;
        }

        var overlapping = _areaOfEffectArea.GetOverlappingAreas();
        foreach (var node in overlapping)
        {
            if (node is not BotHitbox hitbox)
            {
                return;
            }

            HandleHitEnemy(hitbox.EnemyOwner);
        }

        PierceCount++;
        if (PierceLimit != -1 && PierceCount >= PierceLimit)
        {
            QueueFree();
        }

        HasHitThisFrame = true;
    }

    protected override void HandleHitEnemy(BaseEnemy enemy)
    {
        EmitSignal(nameof(HitEnemy), enemy);
    }
}