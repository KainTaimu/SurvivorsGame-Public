// namespace Game.Models;

// public class FastCollection<T>(int initialCapacity = 4)
// {
// 	public int Count { get; private set; }
// 	public int Capacity { get; private set; } = initialCapacity;
//
// 	private int[] _indexToId = new int[initialCapacity];
//
// 	/// <summary>
// 	/// Stable index that points to an object's internal index
// 	/// </summary>
// 	private int[] _idToInternalIndex = new int[initialCapacity];
//
// 	private T[] _data = new T[initialCapacity];
//
// 	private int _lastFreeId;
//
// 	public T this[int idx]
// 	{
// 		get => GetByIndex(idx);
// 		set => _data[idx] = value;
// 	}
//
// 	public void Add(T obj)
// 	{
// 		if (Count == Capacity)
// 			GrowInternal();
//
// 		if (_lastFreeId == -1)
// 			_lastFreeId = Count;
//
// 		_data[Count] = obj;
// 		_idToInternalIndex[_lastFreeId] = Count;
// 		_indexToId[Count] = _lastFreeId;
//
// 		Count++;
// 	}
//
// 	public T Get(int id)
// 	{
// 		var internalIdx = _idToInternalIndex[id];
// 		return _data[internalIdx];
// 	}
//
// 	private T GetByIndex(int idx)
// 	{
// 		var id = _indexToId[idx];
// 		var internalIdx = _idToInternalIndex[id];
// 		return _data[internalIdx];
// 	}
//
// 	public void Remove(int id)
// 	{
// 		var internalIndex = _idToInternalIndex[id];
//
// 		// swap with tail
// 		(_data[internalIndex], _data[Count - 1]) = (_data[Count - 1], _data[internalIndex]);
// 		(_idToInternalIndex[id], _indexToId[Count - 1]) = (_indexToId[Count - 1], _idToInternalIndex[id]);
//
// 		Count--;
// 	}
//
// 	private void GrowInternal()
// 	{
// 		Capacity *= 2;
// 		Array.Resize(ref _indexToId, Capacity);
// 		Array.Resize(ref _data, Capacity);
// 	}
//
// 	private struct Object(T data, int index, int id, int next, int previous)
// 	{
// 		public T Data = data;
// 		public int Index = index;
// 		public int Id = id;
// 		public int Next = next;
// 		public int Previous = previous;
// 	}
// }
//
// public struct Token(int id, int version)
// {
// 	internal int Id = id;
// 	internal int Version = id;
// 	public static readonly Token Default = new(-1, -1);
// }
