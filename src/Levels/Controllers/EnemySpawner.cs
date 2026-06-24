using Arch.Core;
using Game.Core;
using Game.Core.ECS;
using Game.Core.Services;

namespace Game.Levels.Controllers;

public partial class EnemySpawner : Node
{
	public static EnemySpawner? Instance;

	public override void _Ready()
	{
		Instance = this;
	}

	public Entity? SpawnEnemy(EnemyBlueprint bp)
	{
		var ss = ServiceLocator.GetService<SpriteFrameMappingsService>();
		if (ss is null)
		{
			Logger.LogError("Could not get sprite frame mappings service");
			return null;
		}

		var pos = GetPositionOutsideViewport();

		var stats = bp.Stats;
		var spriteInfo = ss.GetSpriteInfo(bp.Name);

		var entity = GameWorld.World.Create(
			new HealthComponent(stats.MaxHealth),
			new PositionComponent { Position = pos },
			new AnimatedSpriteComponent
			{
				SpriteName = spriteInfo?.SpriteName ?? "",
				AnimationSpeed = spriteInfo?.AnimationSpeed ?? Mathf.Inf,
				FrameCountX = spriteInfo?.FrameCountX ?? 1,
				FrameCountY = 1,
				FrameSizePxX = spriteInfo?.FrameSizePxX ?? 32,
				FrameSizePxY = spriteInfo?.FrameSizePxY ?? 32,
				Opacity = spriteInfo?.Opacity ?? 255,
				Flash = spriteInfo?.Flash ?? 0,
				Scale = bp.Stats.SpriteScaleMultiplier,
			},
			new CircleHitboxComponent(bp.Stats.SpriteScaleMultiplier * 16f),
			new MoveSpeedComponent(Mathf.CeilToInt(stats.MoveSpeed * stats.MoveSpeedMultiplier)),
			VelocityComponent.Zero,
			new EnemyContactDamageComponent(Mathf.CeilToInt(stats.DamageOnContact * stats.ContactDamageMultiplier)),
			new DeathRewardComponent(Mathf.CeilToInt(stats.MoneyDrop * stats.MoneyDropMultiplier)),
			new HitFeedbackComponent { HitTime = 0 }
		);
		switch (bp.Type)
		{
			case EnemyType.Fodder:
				GameWorld.World.Add(entity, new FodderMarkerComponent());
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}

		return entity;
	}

	private Vector2 GetPositionOutsideViewport()
	{
		var viewport = GameWorld.Instance.GetViewport().GetCamera2D();
		var screenCenterPosition = viewport.GetScreenCenterPosition();
		var viewportRectEnd = viewport.GetViewportRect().Size;

		const float margin = 100;
		var spawnVector = new Vector2();

		switch (GD.RandRange(0, 3))
		{
			case 0: // TOP
				spawnVector.X = (float)
					GD.RandRange(
						screenCenterPosition.X - viewportRectEnd.X / 2 - margin,
						screenCenterPosition.X + viewportRectEnd.X / 2 + margin
					);
				spawnVector.Y = screenCenterPosition.Y - viewportRectEnd.Y / 2 - margin;
				break;

			case 1: // BOTTOM
				spawnVector.X = (float)
					GD.RandRange(
						screenCenterPosition.X - viewportRectEnd.X / 2 - margin,
						screenCenterPosition.X + viewportRectEnd.X / 2 + margin
					);
				spawnVector.Y = screenCenterPosition.Y + viewportRectEnd.Y / 2 + margin;
				break;

			case 2: // LEFT
				spawnVector.X = screenCenterPosition.X - viewportRectEnd.X / 2 - margin;
				spawnVector.Y = (float)
					GD.RandRange(
						screenCenterPosition.Y - viewportRectEnd.Y / 2 - margin,
						screenCenterPosition.Y + viewportRectEnd.Y / 2 + margin
					);
				break;

			case 3: // RIGHT
				spawnVector.X = screenCenterPosition.X + viewportRectEnd.X / 2 + margin;
				spawnVector.Y = (float)
					GD.RandRange(
						screenCenterPosition.Y - viewportRectEnd.Y / 2 - margin,
						screenCenterPosition.Y + viewportRectEnd.Y / 2 + margin
					);
				break;
		}

		return spawnVector;
	}
}
