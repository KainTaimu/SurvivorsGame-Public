using Arch.Core;
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
		var spriteInfo = ss.GetSpriteInfo(bp.SpriteName);

		return GameWorld.World.Create(
			new EnemyTypeComponent(bp.Type),
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
			new MoveSpeedComponent(Mathf.CeilToInt(stats.MoveSpeed * stats.MoveSpeedMultiplier), stats.TurnSpeed),
			VelocityComponent.Zero,
			new EnemyContactDamageComponent(Mathf.CeilToInt(stats.DamageOnContact * stats.ContactDamageMultiplier)),
			new DeathRewardComponent(Mathf.CeilToInt(stats.MoneyDrop * stats.MoneyDropMultiplier)),
			new HitFeedbackComponent { HitTime = 0 },
			new CollisionLodComponent(CollisionLodLevel.Far)
		);
	}

	private Vector2 GetPositionOutsideViewport()
	{
		var viewport = GameWorld.Instance.GetViewport().GetCamera2D();
		var center = viewport.GetScreenCenterPosition();
		var zoom = viewport.Zoom;
		var halfSize = viewport.GetViewportRect().Size / zoom;
		const float margin = 0;

		var edge = GD.RandRange(0, 3);
		return edge switch
		{
			0 => new Vector2(
				(float)GD.RandRange(center.X - halfSize.X - margin, center.X + halfSize.X + margin),
				center.Y - halfSize.Y - margin
			),
			1 => new Vector2(
				(float)GD.RandRange(center.X - halfSize.X - margin, center.X + halfSize.X + margin),
				center.Y + halfSize.Y + margin
			),
			2 => new Vector2(
				center.X - halfSize.X - margin,
				(float)GD.RandRange(center.Y - halfSize.Y - margin, center.Y + halfSize.Y + margin)
			),
			3 => new Vector2(
				center.X + halfSize.X + margin,
				(float)GD.RandRange(center.Y - halfSize.Y - margin, center.Y + halfSize.Y + margin)
			),
			_ => Vector2.Zero,
		};
	}
}
