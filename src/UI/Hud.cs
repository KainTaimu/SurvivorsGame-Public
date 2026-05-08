using Game.Levels.Controllers;
using Game.Players;

namespace Game.UI;

public partial class Hud : Node
{
	[Export]
	private Label? _playerHealthLabel;

	[Export]
	private Label? _enemyCountLabel;

	[Export]
	private Label? _moneyLabel;

	private Player Player => GameWorld.Instance.MainPlayer;
	private CharacterStats Stats => Player.Character.CharacterStats;
	private EnemyWaveController? WaveController => EnemyWaveController.Instance;
	private LevelData? LevelData => LevelData.Instance;

	public override void _Ready()
	{
		LevelData?.OnMoneyChanged += (_) => _moneyLabel?.Text = $"${LevelData.Money}";
		_moneyLabel?.Text = $"${LevelData?.Money}";
	}

	public override void _Process(double delta)
	{
		_playerHealthLabel?.Text = "Health: " + Stats.Health.ToString();
		_enemyCountLabel?.Text = "Enemies: " + WaveController?.Alive.ToString();
	}
}
