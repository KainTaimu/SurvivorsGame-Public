using Game.Levels.Controllers;
using Game.Players;

namespace Game.Items;

public abstract partial class BaseItem : Node
{
	public static Player Player => GameWorld.Instance.MainPlayer;
	public static CharacterStats PlayerStats =>
		GameWorld.Instance.MainPlayer.Character.CharacterStats;
}
