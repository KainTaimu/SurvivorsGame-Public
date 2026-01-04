namespace Game.Models;

public class UniformGridCell<T>
{
    private const int MAX_SIZE = 32;

    public required Vector2I Index;
    public required Vector2I Position;

    public readonly T[] Array = new T[MAX_SIZE];

    public int Count
    {
        get => _count;
        set
        {
            var clamped = Math.Clamp(value, 0, MAX_SIZE);
            _count = clamped;
        }
    }

    private int _count;

    // Drops obj silenty by design
    public void Add(T obj)
    {
        if (Count >= MAX_SIZE)
            return;

        Array[Count] = obj;
        Count++;
    }

    public void Clear()
    {
        Count = 0;
    }
}
