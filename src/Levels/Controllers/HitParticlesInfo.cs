namespace Game.Levels.Controllers;

[GlobalClass]
public partial class HitParticlesInfo : Resource
{
	[Export]
	public int Amount;

	[Export]
	public ParticleProcessMaterial ProcessMaterial = null!;
}
