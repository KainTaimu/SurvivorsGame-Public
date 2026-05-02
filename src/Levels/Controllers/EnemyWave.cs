using System.Collections.Generic;
using Game.Core.ECS;
using Game.Core.Services;
using Godot.Collections;

namespace Game.Levels.Controllers;

[GlobalClass]
public partial class EnemyWave : Resource, IEnemyWave
{
	[Signal]
	public delegate void OnWaveStartEventHandler();

	[Signal]
	public delegate void OnWaveEndEventHandler();

	[Export]
	public Array<EnemyBlueprint> EnemyBlueprints = null!;

	[Export]
	public double Duration = 30;

	[Export]
	public uint MaxMobs = 50;

	[Export]
	public double SpawnMinTime
	{
		get;
		set
		{
			if (value > SpawnMaxTime)
			{
				Logger.LogWarning(
					$"SpawnMinTime ({field}) clamped to SpawnMaxTime ({SpawnMaxTime})"
				);
				field = SpawnMaxTime;
				return;
			}
			field = value;
		}
	} = 0.5;

	[Export]
	public double SpawnMaxTime
	{
		get;
		set
		{
			if (value < SpawnMinTime)
			{
				Logger.LogWarning(
					$"SpawnMaxTime ({field}) clamped to SpawnMinTime ({SpawnMinTime})"
				);
				field = SpawnMinTime;
				return;
			}
			field = value;
		}
	} = 1;

	[Export]
	public int SpawnBatchMin = 1;

	[Export]
	public int SpawnBatchMax = 1;

	public List<int> SpawnedIds => _waveController.SpawnedIds;

	public double SpawnTimeLeft;
	public double WaveTimeLeft;
	private EnemyWaveController _waveController = null!;
	private EntityComponentStore _entities = null!;
	private int _index;

	public void Process(double delta)
	{
		WaveTimeLeft -= delta;
		SpawnTimeLeft -= delta;

		if (WaveTimeLeft <= 0)
		{
			EndWave();
			return;
		}
		if (SpawnTimeLeft <= 0)
		{
			// csharpier-ignore
			for (var i = 0; i < GD.RandRange(SpawnBatchMin, SpawnBatchMax); i++)
				SpawnEnemy();
		}
	}

	public void Initialize(EnemyWaveController waveController)
	{
		_waveController = waveController;
		_entities = waveController.EntityComponentStore;
	}

	public void StartWave(int waveIndex)
	{
		WaveTimeLeft = Duration;
		SpawnTimeLeft = SpawnMaxTime;
		_index = waveIndex;
		EmitSignalOnWaveStart();
		Logger.LogDebug($"New {ToString()}");
	}

	public void EndWave()
	{
		EmitSignalOnWaveEnd();
	}

	public void SpawnEnemy()
	{
		if (_waveController.Alive >= MaxMobs)
			return;

		SpawnTimeLeft = GD.RandRange(SpawnMinTime, SpawnMaxTime);

		var ss = ServiceLocator.GetService<SpriteFrameMappingsService>();
		if (ss is null)
		{
			Logger.LogError("Could not get sprite frame mappings service");
			return;
		}

		var pos = GetPositionOutsideViewport();
		var id = _waveController.TotalSpawned++;
		_waveController.Alive++;
		if (!_entities.RegisterEntity(id))
			return;

		var bp = EnemyBlueprints.PickRandom();
		var stats = bp.Stats;

		var spriteInfo = ss.GetSpriteInfo(bp.Name);

		_entities.RegisterComponent(id, new HealthComponent(stats.MaxHealth));
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
		_entities.RegisterComponent(
			id,
			new MoveSpeedComponent(
				Mathf.CeilToInt(stats.MoveSpeed * stats.MoveSpeedMultiplier)
			)
		);
		_entities.RegisterComponent(
			id,
			new EnemyContactDamageComponent(
				Mathf.CeilToInt(
					stats.DamageOnContact * stats.ContactDamageMultiplier
				)
			)
		);
		_entities.RegisterComponent(
			id,
			new DeathRewardComponent(
				Mathf.CeilToInt(stats.MoneyDrop * stats.MoneyDropMultiplier)
			)
		);

		SpawnedIds.Add(id);
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

	public override string ToString()
	{
		return $"Wave {_index} : {Duration}s duration: {EnemyBlueprints.Count} types";
	}
}

public interface IEnemyWave
{
	void Initialize(EnemyWaveController waveController);
	void Process(double delta);
	void StartWave(int waveIndex);
	void EndWave();
}
