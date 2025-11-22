namespace SurvivorsGame.Items.Offensive;

public interface IReloadable
{
    int MagazineCapacity { get; }
    int MagazineCount { get; }

    void Reload();
}
