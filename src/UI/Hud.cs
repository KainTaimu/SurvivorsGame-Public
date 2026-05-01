using Game.Levels.Controllers;
using Game.Players;

namespace Game.UI;

public partial class Hud : Node
{
	[Export]
	private Label? PlayerHealthLabel;

	[Export]
	private Label? EnemyCountLabel;

	[Export]
	private Label? MoneyLabel;

	private Player? Player => GameWorld.Instance.MainPlayer;
	private CharacterStats? Stats => Player?.Character.CharacterStats;
	private EnemyWaveController? WaveController => EnemyWaveController.Instance;
	private LevelData? LevelData => LevelData.Instance;

	public override void _Ready()
	{
		LevelData?.OnMoneyChanged += (_) =>
			MoneyLabel?.Text = $"${LevelData.Money}";
		MoneyLabel?.Text = $"${LevelData?.Money}";
	}

	public override void _Process(double delta)
	{
		PlayerHealthLabel?.Text = "Health: " + Stats?.Health.ToString();
		EnemyCountLabel?.Text = "Enemies: " + WaveController?.Alive.ToString();
	}
}
