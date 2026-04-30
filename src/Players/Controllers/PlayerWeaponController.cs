using System.Collections.Generic;
using Game.Items.Offensive;

namespace Game.Players.Controllers;

// TODO: How to handle weapons that may need to use both mouse buttons like
// zooming in with snipers or locking on target with missile?
public partial class PlayerWeaponController : Node
{
	[Signal]
	public delegate void OnPrimaryAttackReassignedEventHandler();

	[Signal]
	public delegate void OnSecondaryAttackReassignedEventHandler();

	public IManualAttack? PrimaryAttack
	{
		get;
		private set
		{
			field = value;
			Logger.LogDebug(
				$"{field?.GetType().Name ?? "null"} is primary attack"
			);
			if (field is not null)
				EmitSignalOnPrimaryAttackReassigned();
		}
	}
	public IManualAttack? SecondaryAttack
	{
		get;
		private set
		{
			field = value;
			Logger.LogDebug(
				$"{field?.GetType().Name ?? "null"} is secondary attack"
			);
			if (field is not null)
				EmitSignalOnSecondaryAttackReassigned();
		}
	}

	public readonly List<BaseOffensive> AutomaticOffensives = [];
	public readonly List<IManualAttack> ManualOffensives = [];

	public override void _Ready()
	{
		foreach (var node in GetChildren())
		{
			if (node is not BaseOffensive offensive)
				continue;
			AutomaticOffensives.Add(offensive);
			if (offensive is IManualAttack manualAttack)
			{
				if (PrimaryAttack is null)
				{
					manualAttack.AttackActionString =
						InputMapNames.PrimaryAttack;
					PrimaryAttack = manualAttack;
				}
				else if (SecondaryAttack is null)
				{
					manualAttack.AttackActionString =
						InputMapNames.SecondaryAttack;
					SecondaryAttack = manualAttack;
				}
				else
				{
					offensive.ProcessMode = ProcessModeEnum.Disabled;
				}
				ManualOffensives.Add(manualAttack);
			}
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is not InputEventMouseButton motion)
			return;
		if (@event.IsReleased())
			return;

		switch (motion.ButtonIndex)
		{
			case MouseButton.WheelUp:
				NextManualAttack();
				break;
			case MouseButton.WheelDown:
				PreviousManualAttack();
				break;
		}
	}

	private void NextManualAttack()
	{
		if (PrimaryAttack is null || SecondaryAttack is null)
			return;

		if (ManualOffensives.Count == 2)
		{
			// csharpier-ignore
			(PrimaryAttack.AttackActionString, SecondaryAttack.AttackActionString) =
			(
				SecondaryAttack.AttackActionString, PrimaryAttack.AttackActionString
			);
			(PrimaryAttack, SecondaryAttack) = (SecondaryAttack, PrimaryAttack);
			return;
		}

		var oldPrimary = PrimaryAttack;
		oldPrimary.AttackActionString = null;
		DisableManualOffensive(oldPrimary);

		var newPrimary = SecondaryAttack;
		var newSecondary = ManualOffensives[2];

		ManualOffensives.RemoveAt(0);
		ManualOffensives.Add(oldPrimary);

		PrimaryAttack = newPrimary;
		PrimaryAttack.AttackActionString = InputMapNames.PrimaryAttack;
		SecondaryAttack = newSecondary;
		SecondaryAttack.AttackActionString = InputMapNames.SecondaryAttack;

		EnableManualOffensive(newSecondary);
	}

	private void PreviousManualAttack()
	{
		if (PrimaryAttack is null || SecondaryAttack is null)
			return;

		if (ManualOffensives.Count == 2)
		{
			// csharpier-ignore
			(SecondaryAttack.AttackActionString, PrimaryAttack.AttackActionString) =
			(
				PrimaryAttack.AttackActionString, SecondaryAttack.AttackActionString
			);
			(SecondaryAttack, PrimaryAttack) = (PrimaryAttack, SecondaryAttack);
			return;
		}

		var oldPrimary = PrimaryAttack;
		oldPrimary.AttackActionString = null;
		DisableManualOffensive(oldPrimary);

		var newPrimary = SecondaryAttack;
		var newSecondary = ManualOffensives[2];

		ManualOffensives.RemoveAt(0);
		ManualOffensives.Add(oldPrimary);

		PrimaryAttack = newPrimary;
		PrimaryAttack.AttackActionString = InputMapNames.PrimaryAttack;
		SecondaryAttack = newSecondary;
		SecondaryAttack.AttackActionString = InputMapNames.SecondaryAttack;

		EnableManualOffensive(newSecondary);
	}

	// NOTE:
	// May break if the nodes ProcessMode is was not originally
	// Inherit
	private void EnableManualOffensive(IManualAttack manual)
	{
		var node = (manual as Node)!;
		node.ProcessMode = ProcessModeEnum.Inherit;
		if (node is Node2D node2D)
			node2D.Show();
	}

	private void DisableManualOffensive(IManualAttack manual)
	{
		var node = (manual as Node)!;
		node.ProcessMode = ProcessModeEnum.Disabled;
		if (node is Node2D node2D)
			node2D.Hide();
	}
}
