using System.Collections.Generic;
using Game.Items.Offensive;

namespace Game.Players.Controllers;

public abstract partial class AbstractPlayerWeaponController : Godot.Node
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
		set
		{
			field = value;
			EmitSignalOnPrimaryAttackReassigned();
		}
	}

	public IManualAttack? SecondaryAttack
	{
		get;
		set
		{
			field = value;
			if (field is not null)
				EmitSignalOnSecondaryAttackReassigned();
		}
	}

	public readonly List<BaseOffensive> Offensives = [];
	public readonly List<BaseOffensive> AutomaticOffensives = [];
	public readonly List<IManualAttack> ManualOffensives = [];

	// NOTE:
	// May break if the nodes ProcessMode is was not originally
	// Inherit
	protected void EnableManualOffensive(IManualAttack manual)
	{
		var node = (manual as Godot.Node)!;
		node.ProcessMode = ProcessModeEnum.Inherit;
		if (node is Node2D node2D)
			node2D.Show();
	}

	protected void DisableManualOffensive(IManualAttack manual)
	{
		var node = (manual as Godot.Node)!;
		manual.AttackActionString = null;
		node.ProcessMode = ProcessModeEnum.Disabled;
		if (node is Node2D node2D)
			node2D.Hide();
	}
}
