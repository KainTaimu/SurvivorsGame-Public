namespace Game.Levels.Controllers;

[GlobalClass]
public partial class LevelData : Node
{
	[Signal]
	public delegate void OnMoneyChangedEventHandler(int delta);

	public int Money
	{
		get;
		set
		{
			var old = field;
			field = value;
			EmitSignalOnMoneyChanged(old - value);
		}
	}

	public static LevelData? Instance { get; private set; }

	public override void _Ready()
	{
		Instance = this;
		GameWorldInstance.Instance.LevelData = this;
	}
}
