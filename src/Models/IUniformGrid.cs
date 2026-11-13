namespace Game.Models;

public interface IUniformGrid<T, THandle>
	where THandle : struct, IItemHandle
{
	int Count { get; }

	void EnsureCapacity(int newCapacity);
	bool TryGet(int x, int y, out T result, out THandle handle);
	bool TryGet(THandle handle, out T result);
	THandle Add(int x, int y, T obj);
	void Remove(THandle targetHandle);
	void ClearAll();
	void ClearCell(int x, int y);
	int GetCellCount(int x, int y);
	bool IsValidCell(int x, int y);
	CellEnumerator<T> GetEnumerator(int x, int y);
}
