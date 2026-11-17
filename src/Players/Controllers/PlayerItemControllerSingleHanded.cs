// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Linq;
using Game.Items;

namespace Game.Players.Controllers;

public partial class PlayerItemControllerSingleHanded : AbstractPlayerItemController
{
	public override void _Ready()
	{
		InitializeItemNodes();
		ChildOrderChanged += ReorderItems;
		ChildEnteredTree += node =>
		{
			if (node is not BaseItem o)
				return;
			AddItem(o);
		};
		ChildExitingTree += node =>
		{
			if (node is not BaseItem o)
				return;

			var current = ItemList.Find(o);
			var previous = current?.Previous ?? ItemList.Last;
			if (previous is null)
			{
				RemoveItem(o);
				return;
			}

			ref var prev = ref previous.ValueRef;
			Equipped = prev;
			RemoveItem(o);
		};
	}

	public override void _Input(InputEvent @event)
	{
		if (Input.IsPhysicalKeyPressed(Key.Ctrl))
			return;
		if (@event.IsActionPressed(InputMapNames.NextItem))
			NextItem();
		else if (@event.IsActionPressed(InputMapNames.PreviousItem))
			PreviousItem();
	}

	public override void _Process(double delta)
	{
		Equipped?.Process(delta);
	}

	private void ReorderItems()
	{
		ItemList.Clear();

		foreach (var child in GetChildren())
		{
			if (child is not BaseItem item)
				continue;

			ItemList.AddLast(item);
		}
	}

	private void AddItem(BaseItem item)
	{
		ItemList.AddLast(item);
		Equipped ??= item;
	}

	private void RemoveItem(BaseItem item)
	{
		ItemList.Remove(item);
		if (ItemList.Count == 0)
			Equipped = null;
	}

	private void NextItem()
	{
		if (Equipped is null)
			return;

		var node = ItemList.Find(Equipped);
		var next = node?.Next ?? ItemList.First;

		var nextAttack = next?.Value;
		Equipped = nextAttack;
	}

	private void PreviousItem()
	{
		if (Equipped is null)
			return;

		var node = ItemList.Find(Equipped);
		var previous = node?.Previous ?? ItemList.Last;

		var nextAttack = previous?.Value;
		Equipped = nextAttack;
	}

	private void InitializeItemNodes()
	{
		foreach (var child in GetChildren())
		{
			if (child is not BaseItem item)
				continue;
			ItemList.AddLast(item);
		}

		Equipped = ItemList.FirstOrDefault();
	}
}
