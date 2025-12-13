using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.Text.Json.JsonSerializer;

namespace SurvivorsGame.Levels.Systems;

public partial class GlobalEntityManager : Node
{
    private readonly EntityData _entities = new();

    public GlobalEntityManager()
    {
        if (Instance != null)
        {
            Logger.LogError("Cannot have multiple instances of a singleton!");
            QueueFree();
            return;
        }

        Instance = this;
    }

    public static GlobalEntityManager Instance { get; private set; }

    public override void _Process(double delta)
    {
        // if (Engine.GetProcessFrames() % 60 == 0)
        // {
        //     // GD.Print(Entities.ToString());
        //     Logger.LogDebug($"{Performance.Singleton.GetMonitor(Performance.Monitor.MemoryStatic) * 1e-6:F0}MB");
        // }
    }

    public override void _PhysicsProcess(double delta) { }

    public void RegisterEntity(
        int id,
        Vector2 initialPos,
        SpriteFrames sprite,
        int health,
        int defense,
        int moveSpeed,
        int hitboxRadius,
        int damageboxRadius
    )
    {
        _entities.Add(
            id,
            initialPos,
            sprite,
            health,
            defense,
            moveSpeed,
            hitboxRadius,
            damageboxRadius
        );
    }

    public void UnregisterEntity(int id)
    {
        _entities.Remove(id);
    }

    public Vector2 GetPosition(int id)
    {
        if (!_entities.TryGetPosition(id, out var vec))
        {
            Logger.LogError("Attempted access to invalid entity position. Returning Vector2.Inf");
            return vec;
        }
        return vec;
    }

    public SpriteFrames GetSprite(int id)
    {
        if (!_entities.TryGetSprite(id, out var sprite))
        {
            Logger.LogError("Attempted access to invalid entity sprite frames.");
            return sprite;
        }
        return sprite;
    }

    public void SetPosition(int id, Vector2 vec)
    {
        _entities.SetPosition(id, vec);
    }

    public IEnumerable<Vector2> GetPositions()
    {
        return _entities.GetPositions();
    }

    [HotMethod("Sprites may not be contiguous in memory.")]
    public IEnumerable<SpriteFrames> GetSprites()
    {
        return _entities.GetSprites();
    }

    public List<int> GetIds()
    {
        return _entities.GetIds();
    }

    private class EntityData
    {
        private const int INITIALSIZE = 10_000;
        private readonly Dictionary<int, int> _idToIndexTable = []; // {Id: Index to position}
        private readonly Dictionary<int, int> _IndexToIdTable = []; // {Index to position: Id}
        public bool[] Bitset { get; private set; } = new bool[INITIALSIZE];

        public Vector2[] Positions { get; private set; } = new Vector2[INITIALSIZE];
        public SpriteFrames[] Sprites { get; private set; } = new SpriteFrames[INITIALSIZE];
        public int[] Healths { get; private set; } = new int[INITIALSIZE];
        public int[] Defenses { get; private set; } = new int[INITIALSIZE];
        public int[] MoveSpeeds { get; private set; } = new int[INITIALSIZE];
        public int[] HitboxRadius { get; private set; } = new int[INITIALSIZE];
        public int[] DamageboxRadius { get; private set; } = new int[INITIALSIZE];

        public EntityData() { }

        public override string ToString()
        {
            var str = new StringBuilder();
            str.AppendLine($"_idToPositionIndexTable:\n{Serialize(_idToIndexTable.ToList())}");
            str.AppendLine($"_positionIndexToIdTable:\n{Serialize(_IndexToIdTable.ToList())}");
            str.Append("_positions:\n[");
            foreach (var vec in Positions)
            {
                str.Append($"{vec}, ");
            }
            str.Append("]\n");

            str.AppendLine($"_usedPositions:\n{Serialize(Bitset.ToList())}");
            return str.ToString();
        }

        public void Add(
            int id,
            Vector2 initialPos,
            SpriteFrames sprite,
            int health,
            int defense,
            int moveSpeed,
            int hitboxRadius,
            int damageboxRadius
        )
        {
            var index = Array.FindIndex(Bitset, b => !b);
            if (index == -1)
            {
                Grow();
                index = Array.FindIndex(Bitset, b => !b);
            }

            _idToIndexTable[id] = index;
            _IndexToIdTable[index] = id;
            Bitset[index] = true;

            Positions[index] = initialPos;
            Sprites[index] = sprite;
            Healths[index] = health;
            Defenses[index] = defense;
            MoveSpeeds[index] = moveSpeed;
            HitboxRadius[index] = hitboxRadius;
            DamageboxRadius[index] = damageboxRadius;
        }

        public void Remove(int id)
        {
            if (!_idToIndexTable.TryGetValue(id, out var index))
                return;

            Positions[index] = default;
            Bitset[index] = false;

            _idToIndexTable.Remove(id);
            _IndexToIdTable.Remove(index);

            Positions[index] = default;
            Sprites[index] = default;
            Healths[index] = default;
            Defenses[index] = default;
            MoveSpeeds[index] = default;
            HitboxRadius[index] = default;
            DamageboxRadius[index] = default;
        }

        // Returns Vector2.Inf if accessing Position of deleted entity
        public bool TryGetPosition(int id, out Vector2 vec)
        {
            var index = _idToIndexTable[id];
            if (!Bitset[index])
            {
                vec = Vector2.Inf;
                return false;
            }
            vec = Positions[index];
            return true;
        }

        public bool TryGetSprite(int id, out SpriteFrames sprite)
        {
            var index = _idToIndexTable[id];
            if (!Bitset[index])
            {
                sprite = null;
                return false;
            }
            sprite = Sprites[index];
            return true;
        }

        public bool TryGetHealth(int id, out int health)
        {
            var index = _idToIndexTable[id];
            if (!Bitset[index])
            {
                health = default;
                return false;
            }
            health = Healths[index];
            return true;
        }

        public bool TryGetDefense(int id, out int defense)
        {
            var index = _idToIndexTable[id];
            if (!Bitset[index])
            {
                defense = default;
                return false;
            }
            defense = Defenses[index];
            return true;
        }

        public bool TryGetMoveSpeed(int id, out int moveSpeed)
        {
            var index = _idToIndexTable[id];
            if (!Bitset[index])
            {
                moveSpeed = default;
                return false;
            }
            moveSpeed = MoveSpeeds[index];
            return true;
        }

        public void SetPosition(int id, Vector2 vec)
        {
            if (!Bitset[_idToIndexTable[id]])
            {
                Logger.LogError("Could not set position: Unknown id {id}");
                return;
            }
            Positions[_idToIndexTable[id]] = vec;
        }

        public void SetSprite(int id, SpriteFrames sprite)
        {
            if (!Bitset[_idToIndexTable[id]])
            {
                Logger.LogError("Could not set sprite: Unknown id {id}");
                return;
            }
            Sprites[_idToIndexTable[id]] = sprite;
        }

        public void SetHealth(int id, int health)
        {
            if (!Bitset[_idToIndexTable[id]])
            {
                Logger.LogError("Could not set health: Unknown id {id}");
                return;
            }
            Healths[_idToIndexTable[id]] = health;
        }

        public void SetDefense(int id, int defense)
        {
            if (!Bitset[_idToIndexTable[id]])
            {
                Logger.LogError("Could not set defense: Unknown id {id}");
                return;
            }
            Defenses[_idToIndexTable[id]] = defense;
        }

        public void SetMoveSpeed(int id, int moveSpeed)
        {
            if (!Bitset[_idToIndexTable[id]])
            {
                Logger.LogError("Could not set move speed: Unknown id {id}");
                return;
            }
            Defenses[_idToIndexTable[id]] = moveSpeed;
        }

        public void SetHitboxRadius(int id, int radius)
        {
            if (!Bitset[_idToIndexTable[id]])
            {
                Logger.LogError("Could not set hitbox radius: Unknown id {id}");
                return;
            }
            HitboxRadius[_idToIndexTable[id]] = radius;
        }

        public void SetDamageboxRadius(int id, int radius)
        {
            if (!Bitset[_idToIndexTable[id]])
            {
                Logger.LogError("Could not set hitbox radius: Unknown id {id}");
                return;
            }
            DamageboxRadius[_idToIndexTable[id]] = radius;
        }

        public IEnumerable<Vector2> GetPositions()
        {
            for (var i = 0; i < Bitset.GetUpperBound(0); i++)
            {
                if (!Bitset[i])
                    continue;
                yield return Positions[i];
            }
        }

        public IEnumerable<SpriteFrames> GetSprites()
        {
            for (var i = 0; i < Bitset.GetUpperBound(0); i++)
            {
                if (!Bitset[i])
                    continue;
                yield return Sprites[i];
            }
        }

        public IEnumerable<int> GetHealths()
        {
            for (var i = 0; i < Bitset.GetUpperBound(0); i++)
            {
                if (!Bitset[i])
                    continue;
                yield return Healths[i];
            }
        }

        public IEnumerable<int> GetDefenses()
        {
            for (var i = 0; i < Bitset.GetUpperBound(0); i++)
            {
                if (!Bitset[i])
                    continue;
                yield return Defenses[i];
            }
        }

        public IEnumerable<int> GetMoveSpeeds()
        {
            for (var i = 0; i < Bitset.GetUpperBound(0); i++)
            {
                if (!Bitset[i])
                    continue;
                yield return MoveSpeeds[i];
            }
        }

        public List<int> GetIds()
        {
            return [.. _idToIndexTable.Keys];
        }

        private void Grow()
        {
            var newLen = Positions.Length * 2;
            var newBitSetArr = new bool[newLen];
            var newPositionArr = new Vector2[newLen];
            var newSpriteArr = new SpriteFrames[newLen];
            var newHealthArr = new int[newLen];
            var newDefenseArr = new int[newLen];
            var newMoveSpeedArr = new int[newLen];
            var newHitboxRadius = new int[newLen];
            var newDamageboxRadius = new int[newLen];

            var newArrayPointer = 0;
            for (var i = 0; i < Positions.Length; i++)
            {
                if (!Bitset[i])
                    continue;
                var id = _IndexToIdTable[i];
                _IndexToIdTable.Remove(i);
                var index = _idToIndexTable[id];

                newBitSetArr[newArrayPointer] = Bitset[index];
                newPositionArr[newArrayPointer] = Positions[index];
                newSpriteArr[newArrayPointer] = Sprites[index];
                newHealthArr[newArrayPointer] = Healths[index];
                newDefenseArr[newArrayPointer] = Defenses[index];
                newMoveSpeedArr[newArrayPointer] = MoveSpeeds[index];
                newHitboxRadius[newArrayPointer] = HitboxRadius[index];
                newDamageboxRadius[newArrayPointer] = DamageboxRadius[index];

                _idToIndexTable[id] = newArrayPointer;
                _IndexToIdTable[newArrayPointer] = id;

                newArrayPointer++;
            }
            Bitset = newBitSetArr;
            Positions = newPositionArr;
            Sprites = newSpriteArr;
            Healths = newHealthArr;
            Defenses = newDefenseArr;
            MoveSpeeds = newMoveSpeedArr;
            HitboxRadius = newHitboxRadius;
            DamageboxRadius = newDamageboxRadius;
        }
    }
}
