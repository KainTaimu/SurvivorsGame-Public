namespace SurvivorsGame.Entities.Enemies;

public partial class You : BaseEnemy
{
    [ExportCategory("Additional")]
    [Export]
    private GpuParticles2D _particles2d;
}

