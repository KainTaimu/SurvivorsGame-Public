// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

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

			var current = OffensiveList.Find(o);
			var previous = current?.Previous ?? OffensiveList.Last;
			if (previous is null)
			{
				RemoveWeapon(o);
				return;
			}

			ref var prev = ref previous.ValueRef;
			PrimaryAttack = prev;
			RemoveWeapon(o);
		};
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

	public override void _Process(double delta)
	{
		PrimaryAttack?.Process(delta);
	}

	private void ReorderWeapons()
	{
		OffensiveList.Clear();

		foreach (var child in GetChildren())
		{
			if (child is not BaseOffensive offensive)
				continue;

			OffensiveList.AddLast(offensive);
		}
	}

	private void AddWeapon(BaseOffensive offensive)
	{
		OffensiveList.AddLast(offensive);
		PrimaryAttack ??= offensive;
	}

	private void RemoveWeapon(BaseOffensive offensive)
	{
		OffensiveList.Remove(offensive);
		if (OffensiveList.Count == 0)
			PrimaryAttack = null;
	}

	private void NextWeapon()
	{
		if (PrimaryAttack is null)
			return;

		var node = OffensiveList.Find(PrimaryAttack);
		var next = node?.Next ?? OffensiveList.First;

		var nextAttack = next?.Value;
		PrimaryAttack = nextAttack;
	}

	private void PreviousWeapon()
	{
		if (PrimaryAttack is null)
			return;

		var node = OffensiveList.Find(PrimaryAttack);
		var previous = node?.Previous ?? OffensiveList.Last;

		var nextAttack = previous?.Value;
		PrimaryAttack = nextAttack;
	}

	private void InitializeWeaponNodes()
	{
		foreach (var child in GetChildren())
		{
			if (child is not BaseOffensive offensive)
				continue;
			OffensiveList.AddLast(offensive);
		}

		PrimaryAttack = OffensiveList.FirstOrDefault();
	}
}
