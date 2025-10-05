namespace SurvivorsGame.Items.Projectiles;

public partial class ProjectileBullet : BaseProjectile
{
    public override void _Ready()
    {
        base._Ready();
        var tweenSpeed = CreateTween().BindNode(this).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.In);
        var tweenScale = CreateTween().BindNode(this).SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.In);
        var originalSpeed = ProjectileSpeed;

        ProjectileSpeed = originalSpeed * 0.5f;
        tweenSpeed.TweenProperty(this, nameof(ProjectileSpeed), originalSpeed, 0.13f);
        tweenScale.TweenProperty(this, "scale", new Vector2(3, 1), 0.13f);
    }
}