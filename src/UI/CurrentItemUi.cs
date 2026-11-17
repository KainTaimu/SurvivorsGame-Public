// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Linq;
using Game.Core;
using Game.Items;
using Game.Items.Offensive;
using Game.Players.Controllers;
using YARD;

namespace Game.UI;

public partial class CurrentItemUi : CanvasLayer
{
	[Export]
	private AbstractPlayerItemController _itemController = null!;

	[Export]
	private PackedScene _itemCarouselItemScene = null!;

	[Export]
	private ItemCarousel _itemCarousel = null!;

	[Export]
	private Label _equippedItemAmmo = null!;

	[Export]
	private StringName _itemsRegistryPath = new("uid://cafl4rhi4lyju");

	private static Registry<Resource> _registry = null!;

	private BaseItem? _item;

	public override void _EnterTree()
	{
		_registry = new Registry<Resource>(GD.Load(_itemsRegistryPath));
	}

	public override void _Ready()
	{
		PauseController.Instance.OnPauseToggled += OnInstanceOnOnPauseToggled;
		_itemController.OnEquippedReassigned += OnEquippedReassigned;
		_itemController.OnItemListChanged += _ => UpdateCarousel();
		_itemController.ChildOrderChanged += UpdateCarousel;
		OnEquippedReassigned(_itemController.Equipped, null);
	}

	private void OnEquippedReassigned(BaseItem? n, BaseItem? _)
	{
		if (_item is IHasMagazineStats magazineStats1)
			magazineStats1.MagazineStats.OnMagazineCountChanged -= MagazineStatsOnOnMagazineCountChanged;
		if (_item is IReloadable reloadable1)
		{
			reloadable1.ReloadBehaviour.OnReloadStart -= UpdateAmmoCount;
			reloadable1.ReloadBehaviour.OnReloadEnd -= UpdateAmmoCount;
			reloadable1.ReloadBehaviour.OnReloadProgress -= OnReloadBehaviourOnOnReloadProgress;
		}

		_item = n;

		if (_item is IHasMagazineStats magazineStats)
			magazineStats.MagazineStats.OnMagazineCountChanged += MagazineStatsOnOnMagazineCountChanged;
		if (_item is IReloadable reloadable)
		{
			reloadable.ReloadBehaviour.OnReloadStart += UpdateAmmoCount;
			reloadable.ReloadBehaviour.OnReloadEnd += UpdateAmmoCount;
			reloadable.ReloadBehaviour.OnReloadProgress += OnReloadBehaviourOnOnReloadProgress;
		}

		UpdateAmmoCount();
		UpdateCarousel();
	}

	public override void _ExitTree()
	{
		PauseController.Instance.OnPauseToggled -= OnInstanceOnOnPauseToggled;
	}

	private void UpdateAmmoCount()
	{
		var equipped = _itemController.Equipped;
		if (equipped is not IReloadable reloadable)
		{
			_equippedItemAmmo.Hide();
			return;
		}

		_equippedItemAmmo.Show();

		switch (reloadable.ReloadDisplayType)
		{
			case ReloadDisplayType.Normal:
			{
				if (reloadable.IsReloading)
					_equippedItemAmmo.Text = "Reloading...";
				else
					_equippedItemAmmo.Text = CreateAmmoDisplayString(reloadable);
				return;
			}
			case ReloadDisplayType.Progressive:
				_equippedItemAmmo.Text = CreateAmmoDisplayString(reloadable);
				return;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	private static string CreateAmmoDisplayString(IReloadable reloadable)
	{
		if (reloadable.MagazineCount > reloadable.MagazineCapacity)
		{
			var extra = reloadable.MagazineCount - reloadable.MagazineCapacity;
			return $"{reloadable.MagazineCount - extra}+{extra}/{reloadable
				.MagazineCapacity}";
		}

		return $"{reloadable.MagazineCount}/{reloadable.MagazineCapacity}";
	}

	private void UpdateCarousel()
	{
		try
		{
			var countChanged = _itemCarousel.ItemCount != _itemController.Items.Count;
			_itemCarousel.EnsureItemCount<ItemCarouselItem>(_itemController.Items.Count, _itemCarouselItemScene);

			var i = 0;
			var selectedIndex = 0;
			foreach (var item in _itemController.Items)
			{
				if (_itemCarousel.GetItem(i) is not ItemCarouselItem itemCarouselItem)
				{
					i++;
					continue;
				}

				var itemScene = GD.Load<PackedScene>(item.SceneFilePath);
				var stringId = _registry.Filter("scene", itemScene).SingleOrDefault();
				if (stringId is null)
				{
					CustomLogger.LogError(
						$"no entry with property \"scene\" matches SceneFilePath \"{item.SceneFilePath}\""
					);
					return;
				}

				// entry is a OffensiveRegistryEntry
				var entry = _registry.LoadEntry(stringId);
				var properties = (BaseItemProperties)entry.Get("properties");
				itemCarouselItem.ItemName.Text = properties.Name;

				if (_itemController.Equipped == item)
					selectedIndex = i;

				itemCarouselItem.Name = i++.ToString();
			}

			_itemCarousel.RefreshLayout();
			_itemCarousel.SelectIndex(selectedIndex, !countChanged);
		}
		catch (ObjectDisposedException)
		{
			// Ignore disposal during scene reload
		}
	}

	private void OnInstanceOnOnPauseToggled(bool toggle)
	{
		Visible = !toggle;
	}

	private void OnReloadBehaviourOnOnReloadProgress(int i, int i1, int i2)
	{
		UpdateAmmoCount();
	}

	private void MagazineStatsOnOnMagazineCountChanged(int i, int i1)
	{
		UpdateAmmoCount();
	}
}
