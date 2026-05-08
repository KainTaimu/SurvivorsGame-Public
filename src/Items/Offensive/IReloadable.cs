namespace Game.Items.Offensive;

public interface IReloadable
{
	int MagazineCapacity { get; }
	int MagazineCount { get; }
	bool IsReloading { get; }

	void Reload();
}
