using System.Collections.Generic;

namespace SurvivorsGame.Levels.Systems;

public class GridCell
{
    private const int MaxCapacity = 64;

    public readonly List<Node2D> Objects = [];

    public Vector2I Index;

    public Vector2I Position;

    public override string ToString()
    {
        return Index.ToString();
    }

    public void AddObject(Node2D obj)
    {
        if (Objects.Count > MaxCapacity && MaxCapacity != -1)
        {
            Objects.RemoveAt(MaxCapacity);
        }

        Objects.Add(obj);
    }

    public void Clear()
    {
        Objects.Clear();
    }
}

