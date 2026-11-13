using System.Collections.Generic;
using Game.Items.Offensive;

namespace Game.Players.Controllers;

public abstract partial class AbstractPlayerWeaponController : Node
{
	[Signal]
	public delegate void OnOffensiveListChangedEventHandler(BaseOffensive newOffensive);

	[Signal]
	public delegate void OnPrimaryAttackReassignedEventHandler();

	public BaseOffensive? PrimaryAttack
	{
		get;
		protected set
		{
			field?.EmitSignal(BaseOffensive.SignalName.OnUnequipped);
			field = value;
			EmitSignalOnPrimaryAttackReassigned();
		}
	}

	public LinkedList<BaseOffensive> Offensives => OffensiveList;

	protected readonly LinkedList<BaseOffensive> OffensiveList = [];
}
