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

			var current = _manualOffensives.Find(m);
			var previous = current?.Previous ?? _manualOffensives.Last;
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
		ChildOrderChanged += ReorderWeapons;
	}

	private void ReorderWeapons()
	{
		_offensives.Clear();
		_manualOffensives.Clear();

		foreach (var child in GetChildren())
		{
			if (child is not BaseOffensive offensive)
				continue;

			_offensives.Add(offensive);
			if (offensive is IManualAttack m)
				_manualOffensives.AddLast(m);
		}
	}

	private void AddWeapon(BaseOffensive offensive)
	{
		if (offensive is IManualAttack m)
		{
			_manualOffensives.AddLast(m);
			if (PrimaryAttack is null)
			{
				PrimaryAttack = m;
				EnableManualOffensive(m);
			}
		}
		_offensives.Add(offensive);
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
			_manualOffensives.Remove(m);
		}
		_offensives.Remove(offensive);
	}

	public override void _Input(InputEvent @event)
	{
		if (Input.IsPhysicalKeyPressed(Key.Ctrl))
			return;
		if (Input.IsActionPressed(InputMapNames.NextWeapon))
			NextWeapon();
		else if (Input.IsActionPressed(InputMapNames.PreviousWeapon))
			PreviousWeapon();
	}

	private void NextWeapon()
	{
		if (PrimaryAttack is null)
			return;
		if (ManualOffensives.Count <= 1)
			return;

		DisableManualOffensive(PrimaryAttack);
		var node = _manualOffensives.Find(PrimaryAttack);
		var next = node?.Next ?? _manualOffensives.First;

		var nextAttack = next.Value;
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
		var node = _manualOffensives.Find(PrimaryAttack);
		var previous = node?.Previous ?? _manualOffensives.Last;

		var nextAttack = previous.Value;
		PrimaryAttack = nextAttack;
		EnableManualOffensive(PrimaryAttack);
	}

	private void InitializeWeaponNodes()
	{
		foreach (var child in GetChildren())
		{
			if (child is not BaseOffensive offensive)
				continue;
			_offensives.Add(offensive);
			switch (offensive)
			{
				case IManualAttack m:
					_manualOffensives.AddLast(m);
					DisableManualOffensive(m);
					break;
				default:
					throw new NotImplementedException();
			}
		}

		PrimaryAttack = _manualOffensives.First();
		EnableManualOffensive(PrimaryAttack);
	}
}
