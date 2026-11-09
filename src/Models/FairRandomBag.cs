using System.Collections;
using System.Collections.Generic;

namespace Game.Models;

/// <summary>
/// A random collection
/// </summary>
/// <typeparam name="T"></typeparam>
public class FairRandomBag<T> : IEnumerable
{
	public int Count => _pool.Count;

	private readonly Queue<T> _pool = [];

	public IEnumerator GetEnumerator()
	{
		return _pool.GetEnumerator();
	}

	public FairRandomBag(IEnumerable<T> objects)
	{
		foreach (var o in objects) { }
	}
}

public class FairRandomGenerator { }
