namespace Game.Models;

public class UniformGridCell<T>
{
	private const int MAX_SIZE = 32;

	public required Vector2I Index;
	public required Vector2I Position;

	public T[] Array = new T[MAX_SIZE];

	public int Count
	{
		get => _count;
		set
		{
			_count = Math.Clamp(value, 0, MAX_SIZE);
		}
	}

	private int _count;

	// Drops obj silenty by design
	public void Add(T obj)
	{
		if (Count >= MAX_SIZE)
		{
			var newArray = new T[Count * 2];
			Array.CopyTo(newArray, 0);
			Array = newArray;
		}

		Array[Count] = obj;
		Count++;
	}

	public void Clear()
	{
		Count = 0;
	}
}
