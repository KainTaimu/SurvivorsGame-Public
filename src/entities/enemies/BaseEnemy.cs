using SurvivorsGame.Entities.Enemies.States;
using SurvivorsGame.Systems;

namespace SurvivorsGame.Entities.Enemies;

public partial class BaseEnemy : Node2D
{
    [Signal]
    public delegate void EnemyHitEventHandler(BaseEffect effect);

    private Timer _timer = new() { WaitTime = 1, Autostart = true };

    public int Id;

    [ExportCategory("Components")]
    [Export]
    public AnimatedSprite2D Sprite { get; private set; }

    [Export]
    public BotStatController BotStatController { get; private set; }

    [Export]
    public BotHitController BotHitController { get; private set; }

    [Export]
    public VisibleOnScreenNotifier2D VisibleOnScreenNotifier { get; private set; }

    [Export]
    public StateMachine StateMachine { get; private set; }

    [Export]
    public BotHitbox BotHitbox { get; private set; }

    [Export]
    public BotDamageBox BotDamageBox { get; private set; }

    public override void _Ready()
    {
        if (BotStatController == null)
        {
            Logger.LogError($"{GetParent().Name} has no StatController!");
        }

        if (Sprite == null)
        {
            Logger.LogError($"{GetParent().Name} has no assigned AnimatedSprite2D!");
        }

        _timer.Timeout += IsOffScreen;
        InitializeEnemy();
        InitializeAnimations();
    }

    public override void _Process(double delta)
    {
    }

    public override void _PhysicsProcess(double delta)
    {
    }

    private void InitializeEnemy()
    {
        GameWorld.Instance.AddEnemy(this);
    }

    private void InitializeAnimations()
    {
        Sprite.Play();
    }

    private void IsOffScreen()
    {
        switch (VisibleOnScreenNotifier.IsOnScreen())
        {
            case true:
                Sprite.Visible = true;
                break;

            case false:
                Sprite.Visible = false;
                break;
        }
    }
}