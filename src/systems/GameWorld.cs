using System.Collections.Generic;
using SurvivorsGame.Entities.Characters;
using SurvivorsGame.Entities.Enemies;
using SurvivorsGame.Levels;
using SurvivorsGame.Pickups;

namespace SurvivorsGame.Systems;

public partial class GameWorld : Node
{
    [Signal]
    public delegate void PlayerDiedEventHandler();

    public static GameWorld Instance { get; private set; }
    public Player MainPlayer { get; private set; }
    public BaseMap CurrentLevel { get; private set; }
    public List<BaseEnemy> Enemies { get; } = [];
    public Dictionary<string, List<BaseEnemy>> EnemiesByType { get; } = []; // Used by GlobalBotRenderer
    public List<BasePickup> Pickups { get; } = [];
    public int TotalEnemiesSpawned { get; private set; }
    public GameTimeTracker TimeElapsed = new();

    public GameWorld()
    {
        if (Instance != null)
        {
            Logger.LogError("Cannot have multiple instances of a singleton!");
            QueueFree();
            return;
        }

        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
        AddChild(TimeElapsed);
    }

    public void LoadLevel(string levelName)
    {
        // Load level
    }

    public void SetMainPlayer(Player player)
    {
        MainPlayer = player;
    }

    public void SetCurrentLevel(BaseMap level)
    {
        CurrentLevel = level;
    }

    public void AddEnemy(BaseEnemy enemy)
    {
        TotalEnemiesSpawned++;
        enemy.Id = TotalEnemiesSpawned;
        Enemies.Add(enemy);

        if (EnemiesByType.TryGetValue(enemy.GetSceneFilePath(), out var list))
        {
            list.Add(enemy);
            return;
        }

        EnemiesByType.Add(enemy.GetSceneFilePath(), [enemy]);
    }

    public void RemoveEnemy(BaseEnemy enemy)
    {
        Enemies.Remove(enemy);
    }

    public void AddPickup(BasePickup pickup)
    {
        Pickups.Add(pickup);
    }

    public void RemovePickup(BasePickup pickup)
    {
        Pickups.Remove(pickup);
    }

    public partial class GameTimeTracker : Node
    {
        public double StartTime;

        public GameTimeTracker()
        {
            ProcessMode = ProcessModeEnum.Pausable;
        }

        public override string ToString()
        {
            var time = new TimeSpan(0, 0, 0, (int)StartTime).ToString(@"m\:ss");
            return time;
        }

        public override void _Process(double delta)
        {
            StartTime += delta;
        }
    }
}