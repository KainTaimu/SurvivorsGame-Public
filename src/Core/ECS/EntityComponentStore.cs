using System.Collections.Generic;

namespace Game.Core.ECS;

public abstract partial class EntityComponentStore : Node, IEntityComponentStore
{
	[Signal]
	public delegate void BeforeEntityUnregisteredEventHandler(int id);

	public const int MAX_SIZE = 65_536;

	[MustUseReturnValue]
	public abstract bool RegisterEntity(int id);
	public abstract void UnregisterEntity(int id);

	public abstract void RegisterComponent<T>(int id, T data);
	public abstract void UpdateComponent<T>(int id, T data);

	public abstract bool GetComponent<T>(int id, out T component);
	public abstract IEnumerable<(int, T1)> Query<T1>();
	public abstract IEnumerable<(int, T1, T2)> Query<T1, T2>();
	public abstract IEnumerable<(int, T1, T2, T3)> Query<T1, T2, T3>();
	public abstract bool GetIsAlive(int id);
	public abstract void SetComponent<T>(int id, T newData);
}
