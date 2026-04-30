using Game.Levels.Controllers;
using Game.Players;

namespace Game.UI;

public partial class Hud : Node
{
	[Export]
	private Label? PlayerHealthLabel;

	[Export]
	private Label? EnemyCountLabel;

	private Player? Player => GameWorld.Instance.MainPlayer;
	private CharacterStats? Stats => Player?.Character.CharacterStats;
	private EnemyWaveController? WaveController => EnemyWaveController.Instance;

	public override void _Process(double delta)
	{
		PlayerHealthLabel?.Text = "Health: " + Stats?.Health.ToString();
		EnemyCountLabel?.Text = "Enemies: " + WaveController?.Alive.ToString();
	}
}
