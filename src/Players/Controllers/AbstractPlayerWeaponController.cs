using System.Collections.Generic;
using System.Linq;
using Game.Items.Offensive;

namespace Game.Players.Controllers;

public abstract partial class AbstractPlayerWeaponController : Node
{
	[Signal]
	public delegate void OnOffensiveListChangedEventHandler(BaseOffensive newOffensive);

	[Signal]
	public delegate void OnPrimaryAttackReassignedEventHandler();

	[Signal]
	public delegate void OnSecondaryAttackReassignedEventHandler();

	public IManualAttack? PrimaryAttack
	{
		get;
		protected set
		{
			field = value;
			EmitSignalOnPrimaryAttackReassigned();
		}
	}

	public IManualAttack? SecondaryAttack
	{
		get;
		protected set
		{
			field = value;
			if (field is not null)
				EmitSignalOnSecondaryAttackReassigned();
		}
	}

	public IReadOnlyList<BaseOffensive> Offensives => _offensives;
	public IReadOnlyList<IManualAttack> ManualOffensives => [.. _manualOffensives];

	protected readonly List<BaseOffensive> _offensives = [];
	protected readonly LinkedList<IManualAttack> _manualOffensives = [];

	// NOTE:
	// May break if the nodes ProcessMode is was not originally
	// Inherit
	protected void EnableManualOffensive(IManualAttack? manual)
	{
		if (manual is null)
			return;
		var node = (Node)manual;
		manual.AttackActionString = InputMapNames.PrimaryAttack;
		node.ProcessMode = ProcessModeEnum.Inherit;
		if (node is Node2D node2D)
			node2D.Show();
		var offensive = (manual as BaseOffensive)!;
		offensive.EmitSignal(BaseOffensive.SignalName.OnEquipped);
	}

	protected void DisableManualOffensive(IManualAttack? manual)
	{
		if (manual is null)
			return;
		var node = (Node)manual;
		manual.AttackActionString = null;
		node.ProcessMode = ProcessModeEnum.Disabled;
		if (node is Node2D node2D)
			node2D.Hide();
		var offensive = (manual as BaseOffensive)!;
		offensive.EmitSignal(BaseOffensive.SignalName.OnUnequipped);
	}
}
