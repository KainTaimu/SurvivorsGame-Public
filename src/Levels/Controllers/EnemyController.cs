using System.Collections;
using System.Collections.Generic;
using Game.Models;

namespace Game.Levels.Controllers;

public partial class EnemyController : Node
{
    private const int MAX_SIZE = 10;
    private const int MAX_EFFECT_SIZE = 8;
    private readonly BitArray _alive = new(MAX_SIZE, false);

    /// <summary>
    /// Global positions of each entity
    /// </summary>
    private readonly Vector2[] _positions = new Vector2[MAX_SIZE];
    private readonly int[] _speeds = new int[MAX_SIZE];
    private readonly Vector2[] _scales = new Vector2[MAX_SIZE];
    private readonly float[] _hitboxRadii = new float[MAX_SIZE];
    private readonly string[] _spriteNames = new string[MAX_SIZE];
    private readonly IEffect[,] _effects = new IEffect[MAX_SIZE, MAX_EFFECT_SIZE];

    private readonly Dictionary<int, int> _idToIndexTable = []; // {Id: Index to position}
    private readonly Dictionary<int, int> _indexToIdTable = []; // {Index to position: Id}
    private int _count;

    public override void _Ready()
    {
        var player = GameWorld.Instance.MainPlayer;
        if (player is null)
            return;
        RegisterEnemy(0, player.GlobalPosition, Vector2.One * 50, 50, 0, "duck");
    }

    /// <summary>
    /// Returns true if successfully registered. False otherwise.
    /// </summary>
    /// <remarks>
    /// Does not handle when an enemy is registered with an already existing id.
    /// </remarks>
    public bool RegisterEnemy(
        int id,
        Vector2 pos,
        Vector2 scale,
        int moveSpeed,
        float hitboxRadius,
        string sprite
    )
    {
        int idx;
        for (idx = 0; idx < MAX_SIZE; idx++)
        {
            if (!_alive[idx])
                break;
        }
        if (idx == MAX_SIZE)
            return false;

        _alive[idx] = true;
        _positions[idx] = pos;
        _scales[idx] = scale;
        _speeds[idx] = moveSpeed;
        _hitboxRadii[idx] = hitboxRadius;
        _spriteNames[idx] = sprite;
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

    // ===== Queries =====

    public IEnumerable<(int, Vector2, float)> GetPositionsSpeeds()
    {
        for (var i = 0; i < MAX_SIZE; i++)
        {
            if (!_alive[i])
                continue;
            yield return (i, _positions[i], _speeds[i]);
        }
    }

    public IEnumerable<(Vector2, string)> GetPositionsSprites()
    {
        for (var i = 0; i < MAX_SIZE; i++)
        {
            if (!_alive[i])
                continue;
            yield return (_positions[i], _spriteNames[i]);
        }
    }

    // ===== Sets =====

    public void SetPosition(int id, Vector2 newPos)
    {
        var idx = _idToIndexTable[id];
        if (!_alive[idx])
            return;

        _positions[idx] = newPos;
    }

    public void SetPositionWithIndex(int idx, Vector2 newPos)
    {
        if (!_alive[idx])
            return;

        _positions[idx] = newPos;
    }
}
