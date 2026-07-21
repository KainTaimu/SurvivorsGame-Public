using System.Linq;
using Game.Items.Offensive;

namespace Game.Players.Controllers;

public partial class PlayerWeaponControllerSingleHanded : AbstractPlayerWeaponController
{
	public override void _Ready()
	{
		ChildOrderChanged += InitializeWeaponNodes;
		InitializeWeaponNodes();
	}

	private void InitializeWeaponNodes()
	{
		if (PrimaryAttack is not null)
			DisableManualOffensive(PrimaryAttack);
		PrimaryAttack = null;
		SecondaryAttack = null;

		Offensives.Clear();
		AutomaticOffensives.Clear();
		ManualOffensives.Clear();
		foreach (var node in GetChildren())
		{
			if (node is not BaseOffensive offensive)
				continue;

			switch (offensive)
			{
				case IManualAttack:
					AddManualOffensive(offensive);
					break;
				// ReSharper disable once SuspiciousTypeConversion.Global
				case IAutomaticAttack:
					throw new NotImplementedException();
			}
		}
	}

	private void AddManualOffensive(BaseOffensive offensive)
	{
		if (offensive is not IManualAttack manualAttack)
			return;

		if (PrimaryAttack is null)
		{
			manualAttack.AttackActionString = InputMapNames.PrimaryAttack;
			offensive.ProcessMode = ProcessModeEnum.Inherit;
			PrimaryAttack = manualAttack;
		}
		else
			offensive.ProcessMode = ProcessModeEnum.Disabled;

		ManualOffensives.Add(manualAttack);
		Offensives.Add(offensive);
		EmitSignalOnOffensiveListChanged(offensive);
	}

	public override void _Input(InputEvent @event)
	{
#if DEBUG
		if (Input.IsPhysicalKeyPressed(Key.Ctrl))
			return;
#endif
		if (@event.IsActionPressed(InputMapNames.NextWeapon))
		{
			NextManualAttack();
			return;
		}

		if (@event.IsActionPressed(InputMapNames.PreviousWeapon))
			PreviousManualAttack();
	}

	private void NextManualAttack()
	{
		if (PrimaryAttack is null)
			return;
		if (ManualOffensives.Count <= 1)
			return;

		var oldPrimary = PrimaryAttack;
		oldPrimary.AttackActionString = null;
		DisableManualOffensive(oldPrimary);

		var newPrimary = ManualOffensives[1];
		ManualOffensives.RemoveAt(0);
		ManualOffensives.Add(oldPrimary);

		PrimaryAttack = newPrimary;
		PrimaryAttack.AttackActionString = InputMapNames.PrimaryAttack;

		EnableManualOffensive(newPrimary);
	}

	private void PreviousManualAttack()
	{
		if (PrimaryAttack is null)
			return;
		if (ManualOffensives.Count <= 1)
			return;

		var oldPrimary = PrimaryAttack;
		oldPrimary.AttackActionString = null;
		DisableManualOffensive(oldPrimary);

		var newPrimary = ManualOffensives.Last();
		ManualOffensives.RemoveAt(ManualOffensives.Count - 1);
		ManualOffensives.Insert(0, newPrimary);

		PrimaryAttack = newPrimary;
		PrimaryAttack.AttackActionString = InputMapNames.PrimaryAttack;

		EnableManualOffensive(newPrimary);
	}
}
