using System.Threading.Tasks;
using SurvivorsGame.Systems;

namespace SurvivorsGame.Entities.Enemies.States;

public partial class StateEnemyPlayerDied : State
{
    [Export]
    private Shader _fadeShader;

    private float _moveSpeed;

    [Export]
    private VisibleOnScreenNotifier2D _visibleOnScreenNotifier;

    public override string StateName { get; protected set; } = "playerdied";

    public override void Enter()
    {
        base.Enter();

        SetDeferred("monitorable", false);
        SetDeferred("monitoring", false);
        _moveSpeed = GD.RandRange(60, 80);

        if (!_visibleOnScreenNotifier.IsOnScreen())
        {
            GameWorld.Instance.RemoveEnemy(EnemyOwner);
            QueueFree();
            return;
        }

        _visibleOnScreenNotifier.ScreenExited += FreeOnScreenExit;
    }

    public override void PhysicsUpdate(double delta)
    {
        MoveAwayFromPlayer(delta);
    }

    private void MoveAwayFromPlayer(double delta)
    {
        var moveVector = EnemyOwner.Position.DirectionTo(Player.Position) * (float)delta * _moveSpeed;

        EnemyOwner.Position -= moveVector;

        EnemyOwner.Sprite.FlipH = !(moveVector.X < 0);
    }

    private void FreeOnScreenExit()
    {
        if (_visibleOnScreenNotifier.IsOnScreen())
        {
            return;
        }

        GameWorld.Instance.RemoveEnemy(EnemyOwner);

        _ = Out();
    }

    private async Task Out()
    {
        const float fadeTime = 1f;
        var material = EnemyOwner.Sprite.Material as ShaderMaterial;
        if (material is null)
        {
            return;
        }

        var tween = CreateTween().BindNode(this).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.Out);

        material.SetShader(_fadeShader);
        tween.TweenMethod(Callable.From((float i) => { material.SetShaderParameter("amount", i); }), 1f, 0f, fadeTime);
        await Task.Delay((int)fadeTime * 1000);
        EnemyOwner.QueueFree();
    }
}