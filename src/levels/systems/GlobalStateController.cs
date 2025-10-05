using System.Collections.Generic;
using SurvivorsGame.Entities.Enemies.States;
using SurvivorsGame.Systems;

namespace SurvivorsGame.Levels.Systems;

public partial class GlobalStateController : Node
{
    private const uint PartitionCount = 1;

    private readonly List<StateMachine>[] _stateMachines = new List<StateMachine>[PartitionCount];

    private uint _count;

    public GlobalStateController()
    {
        if (Instance != null)
        {
            Logger.LogError("Cannot have multiple instances of a singleton!");
            QueueFree();
            return;
        }

        Instance = this;
        for (var i = 0; i < PartitionCount; i++)
        {
            _stateMachines[i] = [];
        }
    }

    public PlayerStateCache CachedPlayerState { get; } = new();
    public static GlobalStateController Instance { get; private set; }

    public override void _Process(double delta)
    {
        for (var i = 0; i < PartitionCount; i++)
        {
            foreach (var stateMachine in _stateMachines[i])
            {
                stateMachine.Process(delta);
            }
        }
    }

    // TODO: Lerp positions to hide choppiness
    public override void _PhysicsProcess(double delta)
    {
        var scaledDelta = delta * PartitionCount;
        if (_count == PartitionCount)
        {
            _count = 0;
        }

        foreach (var stateMachine in _stateMachines[_count])
        {
            stateMachine.PhysicsProcess(scaledDelta);
        }

        _count++;

        CallDeferred(MethodName.UpdateCachedPlayerState);
    }

    private void UpdateCachedPlayerState()
    {
        var player = GameWorld.Instance.MainPlayer;
        CachedPlayerState.Position = player.GetPosition();
    }

    public void RegisterStateMachine(StateMachine stateMachine)
    {
        // Partition enemies based on ID to spread out updates between PartitionCount frames
        var id = stateMachine.EnemyOwner.Id;
        var index = id % PartitionCount;
        if (_stateMachines[index].Contains(stateMachine))
        {
            return;
        }

        _stateMachines[index].Add(stateMachine);
    }

    public void UnregisterStateMachine(StateMachine stateMachine)
    {
        var id = stateMachine.EnemyOwner.Id;
        var index = id % PartitionCount;
        if (!_stateMachines[index].Contains(stateMachine))
        {
            return;
        }

        _stateMachines[index].Remove(stateMachine);
    }

    // Commonly used data such as position should be cached
    public class PlayerStateCache
    {
        public Vector2 Position { get; protected internal set; }
    }
}