using System.Collections.Generic;

namespace SurvivorsGame.Levels.Systems;

public class GridCell<T>
{
    private const int MaxCapacity = 64;

    public List<T> Objects = [];

    // HACK: For PlayerVectorFieldGrid
    public T SingleObject;

    public Vector2I Index;

    public Vector2I Position;

    public override string ToString()
    {
        return Index.ToString();
    }

    public void AddObject(T obj)
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
