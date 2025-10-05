using Godot.Collections;
using SurvivorsGame.Entities.Characters;
using SurvivorsGame.Entities.Enemies;
using SurvivorsGame.Systems;

namespace SurvivorsGame.Levels.Systems;

public partial class WaveController : Node
{
    private Array<Wave> _waves = [];
    private Wave _activeWave;
    private int _currentWaveIndex;

    // [Export] private Timer _spawnDurationTimer;
    private double _spawnDurationTime;

    // [Export] private Timer _spawnSpeedTimer;
    private double _spawnSpeedTime;
    [Export] private uint _maxMobCount;

    [ExportCategory("Toggles")] [Export] public bool Enabled = true;
    [Export] private bool _showSpawnerBounds;
    private static Player MainPlayer => GameWorld.Instance.MainPlayer;

    public override void _Ready()
    {
        foreach (var node in GetChildren())
        {
            if (node is not Wave wave)
            {
                continue;
            }

            if (!wave.Enabled)
            {
                Logger.LogWarning($"{wave.Name} is disabled. Skipping.");
                continue;
            }

            _waves.Add(wave);

            Logger.LogDebug(wave);
            _activeWave ??= wave;
        }

        Logger.LogDebug("Active wave: " + _activeWave.Name);
        StartWave();
    }

    public override void _Process(double delta)
    {
        if (_spawnDurationTime <= 0)
        {
            _spawnDurationTime = _activeWave.Duration;
            NextWave();
        }

        _spawnDurationTime -= delta;

        if (_spawnSpeedTime <= 0)
        {
            _spawnSpeedTime = GD.RandRange(_activeWave.SpawnSpeedMin, _activeWave.SpawnSpeedMax);
            OnSpawnSpeedTimerTimeout();
        }

        _spawnSpeedTime -= delta;
    }

    public string GetWaveStats()
    {
        return _activeWave?.ToString();
    }

    public void StartWave()
    {
        if (!Enabled)
        {
            return;
        }

        OnSpawnSpeedTimerTimeout();
    }

    private void NextWave()
    {
        _currentWaveIndex++;

        if (_currentWaveIndex >= 0 && _currentWaveIndex < _waves.Count)
        {
            _activeWave = _waves[_currentWaveIndex];
        }

        Logger.LogDebug($"Next wave: {_activeWave}");
        StartWave();
    }

    public void StopWave()
    {
        Enabled = false;
    }

    private void OnSpawnSpeedTimerTimeout()
    {
        if (GameWorld.Instance.Enemies.Count >= _maxMobCount || !MainPlayer.Alive || !Enabled)
        {
            return;
        }

        if (MainPlayer.Alive && Enabled)
        {
            var enemyType = _activeWave.GetRandomEnemyType();
            var randomPosition = GetPositionOutsideViewport();
            SpawnEnemy(enemyType, randomPosition);
        }
    }

    private Vector2 GetPositionOutsideViewport()
    {
        var viewport = GetViewport().GetCamera2D();
        var screenCenterPosition = viewport.GetScreenCenterPosition();
        var viewportRectEnd = viewport.GetViewportRect().Size;

        const float margin = 100;
        var spawnVector = new Vector2();

        var seed = GD.RandRange(0, 3);

        switch (seed)
        {
            case 0: // TOP
                spawnVector.X = (float)GD.RandRange(screenCenterPosition.X - viewportRectEnd.X / 2 - margin,
                    screenCenterPosition.X + viewportRectEnd.X / 2 + margin);
                spawnVector.Y = screenCenterPosition.Y - viewportRectEnd.Y / 2 - margin;
                break;

            case 1: // BOTTOM
                spawnVector.X = (float)GD.RandRange(screenCenterPosition.X - viewportRectEnd.X / 2 - margin,
                    screenCenterPosition.X + viewportRectEnd.X / 2 + margin);
                spawnVector.Y = screenCenterPosition.Y + viewportRectEnd.Y / 2 + margin;
                break;

            case 2: // LEFT
                spawnVector.X = screenCenterPosition.X - viewportRectEnd.X / 2 - margin;
                spawnVector.Y = (float)GD.RandRange(screenCenterPosition.Y - viewportRectEnd.Y / 2 - margin,
                    screenCenterPosition.Y + viewportRectEnd.Y / 2 + margin);
                break;

            case 3: // RIGHT
                spawnVector.X = screenCenterPosition.X + viewportRectEnd.X / 2 + margin;
                spawnVector.Y = (float)GD.RandRange(screenCenterPosition.Y - viewportRectEnd.Y / 2 - margin,
                    screenCenterPosition.Y + viewportRectEnd.Y / 2 + margin);
                break;
        }

        return spawnVector;
    }

    private void SpawnEnemy(PackedScene enemyScene, Vector2 position)
    {
        var enemy = (BaseEnemy)enemyScene.Instantiate();
        enemy.Position = position;

        AddChild(enemy);

#if DEBUG
        CreateSpawnBox(position);
#endif
    }

    private void CreateSpawnBox(Vector2 position)
    {
        if (!_showSpawnerBounds)
        {
            return;
        }

        var spawnRect = new ReferenceRect
        {
            Position = position,
            Size = new Vector2(50, 50),
            Visible = true,
            EditorOnly = false
        };

        AddChild(spawnRect);
    }
}