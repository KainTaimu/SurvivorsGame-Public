using System.Collections.Generic;
using System.Linq;
using Game.Items;
using Game.Levels.Controllers;
using Game.Players.Controllers;
using Game.UI.Menus;
using Godot.Collections;

namespace Game.UI;

public partial class PickupUi : Control
{
	[Export]
	public Array<PackedScene> AvailableItemScenes = null!;

	[Export]
	private Array<ItemShowcase> _showcases = null!;

	private PlayerWeaponController PlayerWeaponController => GameWorld.Instance.MainPlayer.WeaponController;

	public override void _Ready()
	{
		foreach (var showcase in _showcases)
		{
			showcase.OnItemSelected += (scene, _) =>
			{
				PlayerWeaponController.AddChild(scene.Instantiate());
				HideUi();
			};
		}
		PopulateItemsRandom(_showcases.Count);
		ShowUi();
	}

	public override void _UnhandledKeyInput(InputEvent @event)
	{
		if (@event is not InputEventKey key)
			return;
		if (key.IsReleased() && key.PhysicalKeycode == Key.F2)
		{
			PopulateItemsRandom(_showcases.Count);
			ShowUi();
			return;
		}
	}

	public void ShowUi()
	{
		Show();
		PauseController.Instance?.Lock(this);
		PauseController.Instance?.Pause(this);
	}

	public void HideUi()
	{
		Hide();
		PauseController.Instance?.Unlock(this);
		PauseController.Instance?.Unpause(this);
	}

	public void PopulateItemsRandom(int itemCount)
	{
		foreach (var showcase in _showcases)
		{
			showcase.Hide();
			showcase.Reset();
		}

		var shuffle = new List<PackedScene>(AvailableItemScenes);
		shuffle.Shuffle();

		var itemQueue = new Queue<PackedScene>(shuffle);
		var pickedItems = new HashSet<PackedScene>();
		var assignedShowcases = 0;
		for (var i = 0; i < itemCount; i++)
		{
			if (assignedShowcases >= _showcases.Count)
				break;

			if (!itemQueue.TryDequeue(out var randomItemScene))
				break;

			if (pickedItems.Contains(randomItemScene))
				continue;

			var scene = randomItemScene.Instantiate();
			if (scene is not BaseItem item)
			{
				Logger.LogError($"Failed to instantiate item {scene.GetType().Name}. Does it inherit from BaseItem?");
				continue;
			}
			scene.QueueFree(); // wtf
			pickedItems.Add(randomItemScene);

			var properties = item.Properties;
			var pickedShowcase = _showcases[i];
			pickedShowcase.AssignItem(randomItemScene, properties);
			pickedShowcase.Show();

			assignedShowcases++;
		}
	}
}

public static class Utils
{
	public static void Shuffle<T>(this IList<T> list)
	{
		int n = list.Count;
		Random rnd = new();
		while (n > 1)
		{
			int k = rnd.Next(0, n) % n;
			n--;
			(list[n], list[k]) = (list[k], list[n]);
		}
	}
}
