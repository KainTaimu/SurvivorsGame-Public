using Arch.Core;

namespace Game.Items.Offensive.EnemyHitEffects;

[GlobalClass]
public partial class SlowEnemyHitEffect : AbstractEnemyHitEffect
{
	public float SlowMultiplier { get; set; }

	public override void ApplyEffect(Entity entity)
	{
		OffensiveEffects.ApplyVelocityMultiplier(entity, SlowMultiplier);
	}
}
