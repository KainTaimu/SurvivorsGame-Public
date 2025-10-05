using System.Collections.Generic;
using SurvivorsGame.Levels.Systems;
using SurvivorsGame.Systems;

namespace SurvivorsGame.Entities.Enemies.States;

public partial class StateMachine : Node
{
    [Signal]
    public delegate void StateChangedEventHandler();

    [Export] public BaseEnemy EnemyOwner { get; private set; }
    public State CurrentState { get; private set; }

    private readonly Dictionary<string, State> _states = new();
    [Export] private State _initialState;

    public override void _Ready()
    {
        if (EnemyOwner is null)
        {
            Logger.LogError($"[ERROR] {GetParent().Name}'s StateMachine has no assigned EnemyOwner!");
        }

        foreach (var child in GetChildren())
        {
            if (child is State state)
            {
                _states.Add(state.StateName.ToLower(), state);
                state.Transitioned += OnChildTransition;
            }
            else
            {
                Logger.LogWarning($"A child of {Name} is not a State node.");
            }
        }

        if (CurrentState is null)
        {
            if (_initialState is not null)
            {
                _initialState.Enter();
                CurrentState = _initialState;
            }
            else
            {
                Logger.LogError($"An initial state is not set for {Name}");
            }
        }

        GameWorld.Instance.PlayerDied += PlayerDied;
        GlobalStateController.Instance.RegisterStateMachine(this);
    }

    public override void _ExitTree()
    {
        GlobalStateController.Instance.UnregisterStateMachine(this);
    }

    public State GetCurrentState()
    {
        return CurrentState;
    }

    public void Process(double delta)
    {
        CurrentState?.Update(delta);
    }

    public void PhysicsProcess(double delta)
    {
        CurrentState?.PhysicsUpdate(delta);
    }

    public void OnChildTransition(string toState)
    {
        if (!_states.TryGetValue("dying", out var isDying))
        {
            return;
        }

        if (isDying == CurrentState)
        {
            return;
        }

        if (!_states.TryGetValue(toState.ToLower(), out var newState))
        {
            Logger.LogWarning($"State \"{toState}\" doesn't exist for {EnemyOwner.Name}");
            return;
        }

        CurrentState?.Exit();

        newState.Enter();
        CurrentState = newState;
    }

    private void PlayerDied()
    {
        OnChildTransition("playerdied");
    }
}