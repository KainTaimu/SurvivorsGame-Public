namespace Game.Levels.Controllers;

public partial class FodderController : Node
{
    [Export]
    private EnemyController _centralController = null!;

    public override void _PhysicsProcess(double delta)
    {
        if (GameWorld.Instance.MainPlayer is null)
            return;

        foreach (var (idx, pos, speed) in _centralController.GetPositionsSpeeds())
        {
            var moveVec = pos.MoveToward(
                GameWorld.Instance.MainPlayer.GlobalPosition,
                speed * (float)delta
            );
            _centralController.SetPositionWithIndex(idx, moveVec);
        }
    }
}
