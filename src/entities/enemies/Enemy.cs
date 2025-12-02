using SurvivorsGame.Levels.Systems;

namespace SurvivorsGame.Entities.Enemies;

public partial class Enemy : Node
{
    public int Id;

    [Export]
    public EnemyStats Stats { get; private set; }

    public Vector2 Position
    {
        get => GlobalEntityManager.Instance.GetPosition(Id);
        set => GlobalEntityManager.Instance.SetPosition(Id, value);
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() { }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }
}
