using SurvivorsGame.Systems;

namespace SurvivorsGame.Entities.Characters;

public partial class Player : Area2D
{
    [Signal]
    public delegate void PlayerDamagedEventHandler(int damage);

    [Signal]
    public delegate void PlayerLevelledUpEventHandler();

    [Export] public PlayerMovementController PlayerMovementController { get; private set; }
    [Export] public PlayerStatController StatController { get; private set; }
    [Export] public PlayerOffensiveController OffensiveController { get; private set; }
    [Export] public PlayerPassiveController PassiveController { get; private set; }
    [Export] public PlayerXpController XpController { get; private set; }
    [Export] public AnimatedSprite2D Sprite { get; private set; }
    public bool Alive = true;

    public override void _EnterTree()
    {
        GameWorld.Instance.SetMainPlayer(this);
    }

    public override void _Ready()
    {
        AddToGroup("Player");
    }
}