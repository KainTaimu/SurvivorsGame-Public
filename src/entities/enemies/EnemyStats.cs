namespace SurvivorsGame.Entities.Enemies;

[GlobalClass]
public partial class EnemyStats : Resource
{
    [Export]
    public SpriteFrames SpriteFrames;

    [Export]
    public int Health;

    [Export]
    public int Defense;

    [Export]
    public int MoveSpeed;
}
