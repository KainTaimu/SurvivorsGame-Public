using Arch.Core;

namespace Game.Items.Offensive.EnemyHitEffects;

[GlobalClass]
public abstract partial class AbstractEnemyHitEffect : Resource
{
	public abstract void ApplyEffect(Entity entity);
}
