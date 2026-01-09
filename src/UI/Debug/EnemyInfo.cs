using Game.Levels.Controllers;

namespace Game.UI.Debug;

public partial class EnemyInfo : Node
{
    [Export]
    private Label _label = null!;

    [Export]
    private EnemySpawner? _spawner;

    public override void _Ready()
    {
        UpdateLabels();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        UpdateLabels();
    }

    private void UpdateLabels()
    {
        if (_spawner is null)
            return;

        _label.Text = $"Enemies: {_spawner.Alive}";
    }
}
