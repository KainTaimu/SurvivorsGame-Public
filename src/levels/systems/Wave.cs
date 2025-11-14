using Godot.Collections;

namespace SurvivorsGame.Levels.Systems;

public partial class Wave : Node
{
    [Export]
    public int Duration;

    [Export]
    public bool Enabled = true;

    [Export]
    public Array<PackedScene> EnemyTypes = [];

    [Export]
    public double SpawnSpeedMax = 0.0001;

    [Export]
    public double SpawnSpeedMin = 0.0001;

    public override void _EnterTree()
    {
        if (SpawnSpeedMax < SpawnSpeedMin)
        {
            Logger.LogWarning(
                "Wave maximum spawn timer should be less than its minimum. Clamping to max"
            );
            SpawnSpeedMin = SpawnSpeedMax;
        }
    }

    public override string ToString()
    {
        return $"{Name} : {EnemyTypes.Count} enemy types : ({SpawnSpeedMin}, {SpawnSpeedMax}) spawn speed : {Duration} duration";
    }

    public PackedScene GetRandomEnemyType()
    {
        return EnemyTypes[GD.RandRange(0, EnemyTypes.Count - 1)];
    }
}

