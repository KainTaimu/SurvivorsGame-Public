using System.Collections.Generic;

namespace Game.Core.ECS;

public interface IEntityComponentStore
{
	bool RegisterEntity(int id);
	void UnregisterEntity(int id);

	void RegisterComponent<T>(int id, T data);
	void UnregisterComponent<T>(int id);
	void UpdateComponent<T>(int id, T data);

	bool GetComponent<T>(int id, out T component);
	IEnumerable<(int, T1)> Query<T1>();
	IEnumerable<(int, T1, T2)> Query<T1, T2>();
	IEnumerable<(int, T1, T2, T3)> Query<T1, T2, T3>();
	bool GetIsAlive(int id);
	void SetComponent<T>(int id, T newData);
}
