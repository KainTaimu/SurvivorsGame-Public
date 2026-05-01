using Game.Core.ECS;
using Game.VFX;

namespace Game.Levels.Controllers;

public partial class EnemyHitFeedbackController : Node
{
	[Export]
	private EntityComponentStore ComponentStore = null!;

	public override void _Process(double delta)
	{
		foreach (
			var (id, spr, hit, pos) in ComponentStore.Query<
				AnimatedSpriteComponent,
				HitFeedbackComponent,
				PositionComponent
			>()
		)
		{
			if (hit.HitTimeLeft <= 0)
				continue;
			var newHitTime = Math.Clamp(
				hit.HitTimeLeft - delta,
				0,
				double.MaxValue
			);
			var flash = 255 * (newHitTime / hit.HitTime);

			var newHit = hit with { HitTimeLeft = newHitTime };
			var newFlash = spr with { Flash = (byte)flash };

			ComponentStore.SetComponent(id, newFlash);

			ComponentStore.SetComponent(id, newHit);

			if (hit.Damage <= 0)
				ComponentStore.SetComponent(id, newHit);
			else
			{
				DamageIndicatorPool.Instance?.GetIndicator(
					pos.Position,
					hit.Damage,
					hit.IsCrit
				);

				ComponentStore.SetComponent(id, newHit with { Damage = -1 });
			}
		}
	}
}
