using Game.Core.ECS;

namespace Game.Levels.Controllers;

public partial class EnemyHitFeedbackController : Node
{
	[Export]
	private EntityComponentStore ComponentStore = null!;

	public override void _Process(double delta)
	{
		foreach (
			var (id, spr, hit) in ComponentStore.Query<
				AnimatedSpriteComponent,
				HitFeedbackComponent
			>()
		)
		{
			if (hit.HitTime <= 0)
				continue;
			var hitTime = Math.Clamp(hit.HitTime - delta, 0, double.MaxValue);
			var flash = 255 * hitTime;

			var newHit = hit with { HitTime = hitTime };
			var newFlash = spr with { Flash = (byte)flash };

			ComponentStore.SetComponent(id, newFlash);
			ComponentStore.SetComponent(id, newHit);
		}
	}
}
