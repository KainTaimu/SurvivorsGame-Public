using Game.Core.ECS;
using Game.Core.Settings;
using Game.VFX;

namespace Game.Levels.Controllers;

public partial class EnemyHitFeedbackController : Node
{
	private EntityComponentStore ComponentStore => EntityComponentStore.Instance;

	[Export]
	private GoreManager? _goreManager;

	[Export]
	private AudioStreamPlayer? _hitmarkerStreamPlayer;

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
			var newHitTime = Math.Clamp(hit.HitTimeLeft - delta, 0, double.MaxValue);
			var flash = 255 * (newHitTime / hit.HitTime);

			var newHit = hit with { HitTimeLeft = newHitTime };
			var newFlash = spr with { Flash = (byte)flash };

			ComponentStore.SetComponent(id, newFlash);

			ComponentStore.SetComponent(id, newHit);

			if (hit.Damage <= 0)
				ComponentStore.SetComponent(id, newHit);
			else
			{
				DamageIndicatorPool.Instance?.GetIndicator(pos.Position, hit.Damage, hit.IsCrit);
				if (GameSettings.Instance.GoreEffects >= GoreEffectsEnum.Medium)
				{
					var spurtDirection = GameWorld.Instance.MainPlayer.GlobalPosition.AngleToPoint(pos.Position);
					_goreManager?.SpawnHitSpurtPaticles(pos.Position, spurtDirection);
				}

				ComponentStore.SetComponent(id, newHit with { Damage = -1 });
				_hitmarkerStreamPlayer?.Play();
			}
		}
	}
}
