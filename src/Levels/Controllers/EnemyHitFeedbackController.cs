using Arch.Core;
using Game.Core.ECS;
using Game.Core.Settings;
using Game.VFX;

namespace Game.Levels.Controllers;

public partial class EnemyHitFeedbackController : Node
{
	[Export]
	private GoreManager? _goreManager;

	[Export]
	private AudioStreamPlayer? _hitmarkerStreamPlayer;

	public override void _Process(double delta)
	{
		GameWorld.World.Query<HitFeedbackComponent, PositionComponent, AnimatedSpriteComponent>(
			in new QueryDescription().WithAll<HitFeedbackComponent, PositionComponent, AnimatedSpriteComponent>(),
			(ref hit, ref pos, ref spr) =>
			{
				var newHitTime = hit.HitTimeLeft - delta;
				hit.HitTimeLeft = newHitTime;

				if (hit.HitTimeLeft <= 0)
					return;

				var flash = 255 * (newHitTime / hit.HitTime);
				spr.Flash = (byte)flash;

				if (hit.Damage <= 0)
					return;

				DamageIndicatorPool.Instance?.GetIndicator(pos.Position, hit.Damage, hit.IsCrit);
				if (GameSettings.Instance.GoreEffects >= GoreEffectsEnum.Medium)
				{
					var spurtDirection = GameWorld.Instance.MainPlayer.GlobalPosition.AngleToPoint(pos.Position);
					_goreManager?.SpawnHitSpurtPaticles(pos.Position, spurtDirection);
				}

				hit.Damage = -1;
				_hitmarkerStreamPlayer?.Play();
			}
		);
	}
}
