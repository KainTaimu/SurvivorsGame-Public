using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.Text.Json.JsonSerializer;

namespace SurvivorsGame.Levels.Systems;

public partial class GlobalEntityManager : Node
{
    private readonly EntityData Entities = new();

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

    public void RegisterEntity(int id, Vector2 initialPosition)
    {
        Entities.Add(id, initialPosition);
    }

    public void UnregisterEntity(int id)
    {
        Entities.Remove(id);
    }

    public Vector2 GetPosition(int id)
    {
        if (!Entities.TryGetPosition(id, out var vec))
        {
            Logger.LogWarning("Attempted access to invalid entity position. Returning Vector2.Inf");
            return vec;
        }
        return vec;
    }

    public void SetPosition(int id, Vector2 vec)
    {
        Entities.SetPosition(id, vec);
    }

    public List<Vector2> GetPositions()
    {
        return Entities.GetPositions();
    }

    private class EntityData
    {
        private const int INITIALSIZE = 1_000;
        private readonly Dictionary<int, int> _idToPositionIndexTable = []; // {Id: Index to position}
        private readonly Dictionary<int, int> _positionIndexToIdTable = []; // {Index to position: Id}
        public Vector2[] Positions { get; private set; } = new Vector2[INITIALSIZE];
        public bool[] UsedPositions { get; private set; } = new bool[INITIALSIZE];

        // uint[] HitboxRadius
        // uint[] PlayerDamageBoxRadius
        // Sprite[] Sprites

        public EntityData() { }

        public override string ToString()
        {
            var str = new StringBuilder();
            str.AppendLine(
                $"_idToPositionIndexTable:\n{Serialize(_idToPositionIndexTable.ToList())}"
            );
            str.AppendLine(
                $"_positionIndexToIdTable:\n{Serialize(_positionIndexToIdTable.ToList())}"
            );
            str.Append("_positions:\n[");
            foreach (var vec in Positions)
            {
                str.Append($"{vec}, ");
            }
            str.Append("]\n");

            str.AppendLine($"_usedPositions:\n{Serialize(UsedPositions.ToList())}");
            return str.ToString();
        }

        public void Add(int id, Vector2 initialPos)
        {
            var index = Array.FindIndex(UsedPositions, b => !b);
            if (index == -1)
            {
                Grow();
                index = Array.FindIndex(UsedPositions, b => !b);
            }

            Positions[index] = initialPos;
            UsedPositions[index] = true;

            _idToPositionIndexTable[id] = index;
            _positionIndexToIdTable[index] = id;
        }

        public void Remove(int id)
        {
            if (!_idToPositionIndexTable.TryGetValue(id, out var index))
                return;

            Positions[index] = default;
            UsedPositions[index] = false;

            _idToPositionIndexTable.Remove(id);
            _positionIndexToIdTable.Remove(index);
        }

        // Returns Vector2.Inf if accessing Position of deleted entity
        public bool TryGetPosition(int id, out Vector2 vec)
        {
            var index = _idToPositionIndexTable[id];
            if (!UsedPositions[index])
            {
                vec = Vector2.Inf;
                return false;
            }
            vec = Positions[index];
            return true;
        }

        public void SetPosition(int id, Vector2 vec)
        {
            if (!UsedPositions[_idToPositionIndexTable[id]])
                return;
            Positions[_idToPositionIndexTable[id]] = vec;
        }

        public List<Vector2> GetPositions()
        {
            List<Vector2> positions = [];

            for (var i = 0; i < UsedPositions.GetUpperBound(0); i++)
            {
                if (!UsedPositions[i])
                    continue;
                positions.Add(Positions[i]);
            }
            return positions;
        }

        private void Grow()
        {
            var newLen = Positions.Length * 2;
            // Logger.LogWarning($"Growing from {Positions.Length} to {newLen}");
            var newPositionArr = new Vector2[newLen];
            var newUsedPositionArr = new bool[newLen];

            var newArrayPointer = 0;
            for (var i = 0; i < Positions.Length; i++)
            {
                if (!UsedPositions[i])
                    continue;
                var id = _positionIndexToIdTable[i];
                _positionIndexToIdTable.Remove(i);
                var index = _idToPositionIndexTable[id];

                newPositionArr[newArrayPointer] = Positions[index];
                newUsedPositionArr[newArrayPointer] = UsedPositions[index];

                _idToPositionIndexTable[id] = newArrayPointer;
                _positionIndexToIdTable[newArrayPointer] = id;

                newArrayPointer++;
            }
            Positions = newPositionArr;
            UsedPositions = newUsedPositionArr;

            // var str = new StringBuilder();
            // str.Append("_positions:\n[");
            // foreach (var vec in newPositionArr)
            // {
            //     str.Append($"{vec}, ");
            // }
            // str.Append("]\n");
            //
            // str.AppendLine($"_usedPositions:\n{Serialize(newUsedPositionArr.ToList())}");
            // Logger.LogWarning(str.ToString());
        }
    }
}
