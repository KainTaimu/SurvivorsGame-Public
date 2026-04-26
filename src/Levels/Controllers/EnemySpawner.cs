using Game.Core.ECS;
using Game.Core.Services;
using Godot.Collections;

namespace Game.Levels.Controllers;

[Obsolete]
public partial class EnemySpawner : Node
{
	[ExportCategory("Main")]
	[Export]
	private Array<EnemyBlueprint> _enemyBlueprints = null!;

	[ExportCategory("Configuration")]
	[Export]
	private EntityComponentStore _entities = null!;

	[Export]
	private int _spawnCount = 1;

	[Export]
	private int _spawnBatchCount = 1;

	public int TotalSpawned
	{
		get;
		private set
		{
			// Spawned should not be decremented because we rely on it to create unique ids
			field =
				Math.Clamp(value, field, EntityComponentStore.MAX_SIZE);
		}
	}

	public int Alive
	{
		get;
		private set
		{
			field = Math.Clamp(value, 0, EntityComponentStore.MAX_SIZE);
		}
	}

	private double _t;

	public override void _Ready()
	{
		_entities.BeforeEntityUnregistered += (_) => Alive--;
	}

	public override void _Process(double delta)
	{
		_t -= delta;
		SpawnEnemy();
	}

	public void SpawnEnemy()
	{
		if (
			_t > 0
			|| Alive >= EntityComponentStore.MAX_SIZE
			|| Alive >= _spawnCount
		)
			return;
		_t = 0.05f;

		var ss = ServiceLocator.GetService<SpriteFrameMappingsService>();
		if (ss is null)
		{
			Logger.LogError("Could not get sprite frame mappings service");
			return;
		}

		for (var i = 0; i < _spawnBatchCount; i++)
		{
			var pos = GetPositionOutsideViewport();
			var id = TotalSpawned;
			if (!_entities.RegisterEntity(id))
				continue;

			var bp = _enemyBlueprints.PickRandom();
			var stats = bp.Stats;

			var spriteInfo = ss.GetSpriteInfo(bp.Name);

			// TODO: Enemy blueprints
			_entities.RegisterComponent(
				id,
				new HealthComponent(stats.MaxHealth)
			);
			_entities.RegisterComponent(id, new EntityTypeComponent(bp.Type));
			_entities.RegisterComponent(
				id,
				new PositionComponent() { Position = pos }
			);
			_entities.RegisterComponent(
				id,
				new AnimatedSpriteComponent()
				{
					SpriteName = spriteInfo?.SpriteName ?? "",
					AnimationSpeed = spriteInfo?.AnimationSpeed ?? Mathf.Inf,
					FrameCountX = spriteInfo?.FrameCountX ?? 1,
					FrameCountY = 1,
					FrameSizePxX = spriteInfo?.FrameSizePxX ?? 32,
					FrameSizePxY = spriteInfo?.FrameSizePxY ?? 32,
					Opacity = spriteInfo?.Opacity ?? 255,
					Flash = spriteInfo?.Flash ?? 0,
				}
			);

			TotalSpawned++;
			Alive++;
		}
	}

	private Vector2 GetPositionOutsideViewport()
	{
		var viewport = GetViewport().GetCamera2D();
		var screenCenterPosition = viewport.GetScreenCenterPosition();
		var viewportRectEnd = viewport.GetViewportRect().Size;

		const float margin = 100;
		var spawnVector = new Vector2();

		switch (GD.RandRange(0, 3))
		{
			case 0: // TOP
				spawnVector.X = (float)
					GD.RandRange(
						screenCenterPosition.X
							- (viewportRectEnd.X / 2)
							- margin,
						screenCenterPosition.X
							+ (viewportRectEnd.X / 2)
							+ margin
					);
				spawnVector.Y =
					screenCenterPosition.Y - (viewportRectEnd.Y / 2) - margin;
				break;

			case 1: // BOTTOM
				spawnVector.X = (float)
					GD.RandRange(
						screenCenterPosition.X
							- (viewportRectEnd.X / 2)
							- margin,
						screenCenterPosition.X
							+ (viewportRectEnd.X / 2)
							+ margin
					);
				spawnVector.Y =
					screenCenterPosition.Y + (viewportRectEnd.Y / 2) + margin;
				break;

			case 2: // LEFT
				spawnVector.X =
					screenCenterPosition.X - (viewportRectEnd.X / 2) - margin;
				spawnVector.Y = (float)
					GD.RandRange(
						screenCenterPosition.Y
							- (viewportRectEnd.Y / 2)
							- margin,
						screenCenterPosition.Y
							+ (viewportRectEnd.Y / 2)
							+ margin
					);
				break;

			case 3: // RIGHT
				spawnVector.X =
					screenCenterPosition.X + (viewportRectEnd.X / 2) + margin;
				spawnVector.Y = (float)
					GD.RandRange(
						screenCenterPosition.Y
							- (viewportRectEnd.Y / 2)
							- margin,
						screenCenterPosition.Y
							+ (viewportRectEnd.Y / 2)
							+ margin
					);
				break;
		}

		return spawnVector;
	}
}
