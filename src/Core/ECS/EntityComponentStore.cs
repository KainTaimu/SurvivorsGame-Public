using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Game.Core.ECS;

public partial class EntityComponentStore : Node
{
    private const int MAX_SIZE = 32_000;
    private readonly BitArray _alive = new(MAX_SIZE, false);

    private readonly Dictionary<int, int> _idToIndexTable = []; // {Id: Index to position}
    private readonly Dictionary<int, int> _indexToIdTable = []; // {Index to position: Id}
    private int _count;

    private readonly Dictionary<Type, Array> _components = [];

    /// <summary>
    /// Returns true if successfully registered. False otherwise.
    /// </summary>
    /// <remarks>
    /// Does not handle when an enemy is registered with an already existing id.
    /// </remarks>
    public bool RegisterEntity(int id)
    {
        // Find free index
        int idx;
        for (idx = 0; idx < MAX_SIZE; idx++)
        {
            if (!_alive[idx])
                break;
        }
        if (idx == MAX_SIZE)
        {
            Logger.LogWarning($"Couldn't register entity {id} with index {idx}");
            return false;
        }

        _alive[idx] = true;
        _idToIndexTable[id] = idx;
        _indexToIdTable[idx] = id;

        _count++;
        return true;
    }

    public void UnregisterEnemy(int id)
    {
        if (!_idToIndexTable.ContainsKey(id))
            return;

        var idx = _idToIndexTable[id];
        _idToIndexTable.Remove(id);
        _indexToIdTable.Remove(idx);
        _alive[idx] = false;
        _count--;
    }

    public void RegisterComponent<T>(int id, T data)
    {
        if (!_idToIndexTable.TryGetValue(id, out var idx))
        {
            Logger.LogWarning("Couldn't register component. Entity with id", id, "does not exist");
            return;
        }

        var type = typeof(T);
        if (!_components.ContainsKey(type))
            _components.Add(type, Array.CreateInstance(type, MAX_SIZE));

        _components[type].SetValue(data, idx);
    }

    public void UpdateComponent<T>(int id, T data)
    {
        if (!_idToIndexTable.TryGetValue(id, out var idx))
        {
            Logger.LogWarning("Couldn't update component. Entity with id", id, "does not exist");
            return;
        }

        var type = typeof(T);
        if (!_components.ContainsKey(type))
            return;

        _components[type].SetValue(data, idx);
    }

    // ===== Queries =====
    // Could use generators if we need more queries

    private T[]? GetComponents<T>()
    {
        var type = typeof(T);
        if (!_components.TryGetValue(type, out var collection))
            return null;

        return Unsafe.As<T[]>(collection);
    }

    /// <summary>
    /// Returns the entity ID, and the value of T1
    /// </summary>
    public IEnumerable<(int, T1)> Query<T1>()
    {
        var components = GetComponents<T1>();
        if (components is null)
        {
            Logger.LogError($"Component {typeof(T1).Name} not registered. Breaking.");
            yield break;
        }
        for (var i = 0; i < MAX_SIZE; i++)
        {
            if (!_alive[i])
                continue;
            yield return (_indexToIdTable[i], components[i]);
        }
    }

    public IEnumerable<(int, T1, T2)> Query<T1, T2>()
    {
        var c1 = GetComponents<T1>();
        if (c1 is null)
        {
            Logger.LogError($"Component {typeof(T1).Name} not registered. Breaking.");
            yield break;
        }
        var c2 = GetComponents<T2>();
        if (c2 is null)
        {
            Logger.LogError($"Component {typeof(T2).Name} not registered. Breaking.");
            yield break;
        }
        for (var i = 0; i < MAX_SIZE; i++)
        {
            if (!_alive[i])
                continue;
            yield return (_indexToIdTable[i], c1[i], c2[i]);
        }
    }

    public IEnumerable<(int, T1, T2, T3)> Query<T1, T2, T3>()
    {
        var c1 = GetComponents<T1>();
        if (c1 is null)
        {
            Logger.LogError($"Component {typeof(T1).Name} not registered. Breaking.");
            yield break;
        }
        var c2 = GetComponents<T2>();
        if (c2 is null)
        {
            Logger.LogError($"Component {typeof(T2).Name} not registered. Breaking.");
            yield break;
        }
        var c3 = GetComponents<T3>();
        if (c3 is null)
        {
            Logger.LogError($"Component {typeof(T3).Name} not registered. Breaking.");
            yield break;
        }
        for (var i = 0; i < MAX_SIZE; i++)
        {
            if (!_alive[i])
                continue;
            yield return (_indexToIdTable[i], c1[i], c2[i], c3[i]);
        }
    }

    // ===== Sets =====
}
