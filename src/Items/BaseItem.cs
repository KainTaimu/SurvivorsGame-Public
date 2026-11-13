using Game.Levels.Controllers;
using Game.Players;

namespace Game.Items;

[GlobalClass]
public abstract partial class BaseItem : Node
{
	[Signal]
	public delegate void OnStatsChangedEventHandler();

	[Signal]
	public delegate void OnStatUpgradesChangedEventHandler();

	public static Player Player => GameWorld.Instance.MainPlayer;

	public static CharacterStats PlayerStats => GameWorld.Instance.MainPlayer.Character.CharacterStats;
}
