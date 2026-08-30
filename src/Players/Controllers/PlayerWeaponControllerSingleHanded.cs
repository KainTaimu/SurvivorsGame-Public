using System.Linq;
using Game.Items.Offensive;

namespace Game.Players.Controllers;

public partial class PlayerWeaponControllerSingleHanded : AbstractPlayerWeaponController
{
	public override void _Ready()
	{
		InitializeWeaponNodes();
		ChildOrderChanged += ReorderWeapons;
		ChildEnteredTree += node =>
		{
			if (node is not BaseOffensive o)
				return;
			AddWeapon(o);
		};
		ChildExitingTree += node =>
		{
			if (node is not BaseOffensive o)
				return;
			if (o is not IManualAttack m)
			{
				RemoveWeapon(o);
				return;
			}

			var current = ManualOffensiveList.Find(m);
			var previous = current?.Previous ?? ManualOffensiveList.Last;
			if (previous is null)
			{
				RemoveWeapon(o);
				return;
			}

			ref var prev = ref previous.ValueRef;
			PrimaryAttack = prev;
			EnableManualOffensive(prev);
			RemoveWeapon(o);
		};
	}

	private void ReorderWeapons()
	{
		OffensiveList.Clear();
		ManualOffensiveList.Clear();

		foreach (var child in GetChildren())
		{
			if (child is not BaseOffensive offensive)
				continue;

			OffensiveList.Add(offensive);
			if (offensive is IManualAttack m)
				ManualOffensiveList.AddLast(m);
		}
	}

	private void AddWeapon(BaseOffensive offensive)
	{
		if (offensive is IManualAttack m)
		{
			ManualOffensiveList.AddLast(m);
			if (PrimaryAttack is null)
			{
				PrimaryAttack = m;
				EnableManualOffensive(m);
			}
		}

		OffensiveList.Add(offensive);
	}

	private void RemoveWeapon(BaseOffensive offensive)
	{
		if (offensive is IManualAttack m)
		{
			if (PrimaryAttack == m)
			{
				DisableManualOffensive(m);
				PrimaryAttack = null;
			}

			ManualOffensiveList.Remove(m);
		}

		OffensiveList.Remove(offensive);
	}

	public override void _Input(InputEvent @event)
	{
		if (Input.IsPhysicalKeyPressed(Key.Ctrl))
			return;
		if (@event.IsActionPressed(InputMapNames.NextWeapon))
			NextWeapon();
		else if (@event.IsActionPressed(InputMapNames.PreviousWeapon))
			PreviousWeapon();
	}

	private void NextWeapon()
	{
		if (PrimaryAttack is null)
			return;
		if (ManualOffensives.Count <= 1)
			return;

		DisableManualOffensive(PrimaryAttack);
		var node = ManualOffensiveList.Find(PrimaryAttack);
		var next = node?.Next ?? ManualOffensiveList.First;

		var nextAttack = next?.Value;
		PrimaryAttack = nextAttack;
		EnableManualOffensive(PrimaryAttack);
	}

	private void PreviousWeapon()
	{
		if (PrimaryAttack is null)
			return;
		if (ManualOffensives.Count <= 1)
			return;

		DisableManualOffensive(PrimaryAttack);
		var node = ManualOffensiveList.Find(PrimaryAttack);
		var previous = node?.Previous ?? ManualOffensiveList.Last;

		var nextAttack = previous?.Value;
		PrimaryAttack = nextAttack;
		EnableManualOffensive(PrimaryAttack);
	}

	private void InitializeWeaponNodes()
	{
		foreach (var child in GetChildren())
		{
			if (child is not BaseOffensive offensive)
				continue;
			OffensiveList.Add(offensive);
			switch (offensive)
			{
				case IManualAttack m:
					ManualOffensiveList.AddLast(m);
					DisableManualOffensive(m);
					break;
				default:
					throw new NotImplementedException();
			}
		}

		PrimaryAttack = ManualOffensiveList.FirstOrDefault();
		EnableManualOffensive(PrimaryAttack);
	}
}
