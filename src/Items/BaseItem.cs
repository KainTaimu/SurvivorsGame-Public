using Game.Levels.Controllers;
using Game.Players;

namespace Game.Items;

public abstract partial class BaseItem : Node
{
	[Signal]
	public delegate void OnStatsChangedEventHandler();

	[Signal]
	public delegate void OnStatUpgradesChangedEventHandler();

	[Export]
	public BaseItemProperties Properties = null!;

	[Export]
	public BaseItemStats Stats
	{
		get;
		set
		{
			if (
				// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
				field
					is not null
				&& field.IsConnected(Resource.SignalName.Changed, Callable.From(EmitSignalOnStatsChanged))
			)
				field.Changed -= EmitSignalOnStatsChanged;

			field = value;
			value.Changed += EmitSignalOnStatsChanged;
		}
	} = null!;

	public static Player Player => GameWorld.Instance.MainPlayer;

	public static CharacterStats PlayerStats => GameWorld.Instance.MainPlayer.Character.CharacterStats;
}
