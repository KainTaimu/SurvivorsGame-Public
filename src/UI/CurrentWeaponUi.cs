// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Linq;
using Game.Core;
using Game.Items;
using Game.Items.Offensive;
using Game.Players.Controllers;
using YARD;

namespace Game.UI;

public partial class CurrentWeaponUi : CanvasLayer
{
	[Export]
	private AbstractPlayerWeaponController _weaponController = null!;

	[Export]
	private PackedScene _weaponCarouselItemScene = null!;

	[Export]
	private WeaponCarousel _weaponCarousel = null!;

	[Export]
	private Label _primaryWeaponAmmo = null!;

	[Export]
	private Label _secondaryWeaponAmmo = null!;

	[Export]
	private StringName _weaponRegistryPath = new("uid://cafl4rhi4lyju");

	private static Registry<Resource> _registry = null!;

	public override void _EnterTree()
	{
		_registry = new Registry<Resource>(GD.Load(_weaponRegistryPath));
	}

	public override void _Ready()
	{
		PauseController.Instance.OnPauseToggled += OnInstanceOnOnPauseToggled;
		_weaponController.OnPrimaryAttackReassigned += UpdateCarousel;
		_weaponController.OnOffensiveListChanged += _ => UpdateCarousel();
		_weaponController.ChildOrderChanged += UpdateCarousel;
		Callable.From(UpdateCarousel).CallDeferred();
	}

	public override void _ExitTree()
	{
		PauseController.Instance.OnPauseToggled -= OnInstanceOnOnPauseToggled;
	}

	private void OnInstanceOnOnPauseToggled(bool toggle)
	{
		Visible = !toggle;
	}

	public override void _Process(double delta)
	{
		// PERF: Use signals
		UpdateAmmoCount();
	}

	public void UpdateAmmoCount()
	{
		var primaryAttack = _weaponController.PrimaryAttack;
		if (primaryAttack is not IReloadable primary)
		{
			_primaryWeaponAmmo.Hide();
			return;
		}

		_primaryWeaponAmmo.Show();

		switch (primaryAttack)
		{
			case ICustomReloadDisplay customReloadDisplay:
				switch (customReloadDisplay.ReloadDisplayType)
				{
					case ReloadDisplayType.Normal:
					{
						if (primary.IsReloading)
							_primaryWeaponAmmo.Text = "Reloading...";
						else
							_primaryWeaponAmmo.Text = UpdateAmmoDisplay(primary);
						return;
					}
					case ReloadDisplayType.Progressive:
						_primaryWeaponAmmo.Text = UpdateAmmoDisplay(primary);
						return;
					default:
						throw new ArgumentOutOfRangeException();
				}

			default:
				if (primary.IsReloading)
					_primaryWeaponAmmo.Text = "Reloading...";
				else
					_primaryWeaponAmmo.Text = UpdateAmmoDisplay(primary);

				break;
		}
	}

	private static string UpdateAmmoDisplay(IReloadable primary)
	{
		if (primary.MagazineCount > primary.MagazineCapacity)
		{
			var extra = primary.MagazineCount - primary.MagazineCapacity;
			return $"{primary.MagazineCount - extra}+{extra}/{primary
				.MagazineCapacity}";
		}

		return $"{primary.MagazineCount}/{primary.MagazineCapacity}";
	}

	public void UpdateCarousel()
	{
		// HACK:
		// throws a ObjectDisposedException when reloading due to AbstractPlayerWeaponController implementations
		// using ChildOrderChanged as a signal to emit changes to Nodes. Because it can't differentiate between a
		// weapon node exiting normally and it being freed due to Root reloading
		try
		{
			var countChanged = _weaponCarousel.ItemCount != _weaponController.Offensives.Count;
			_weaponCarousel.EnsureItemCount<WeaponItem>(_weaponController.Offensives.Count, _weaponCarouselItemScene);

			var i = 0;
			var selectedIndex = 0;
			foreach (var weapon in _weaponController.Offensives)
			{
				if (_weaponCarousel.GetItem(i) is not WeaponItem weaponItem)
				{
					i++;
					continue;
				}

				// HACK HACK
				var weaponScene = GD.Load<PackedScene>(weapon.SceneFilePath);
				var stringId = _registry.Filter("scene", weaponScene).SingleOrDefault();
				if (stringId is null)
				{
					CustomLogger.LogError(
						$"no entry with property \"scene\" matches SceneFilePath \"{weapon.SceneFilePath}\""
					);
					return;
				}

				// entry is a OffensiveRegistryEntry
				var entry = _registry.LoadEntry(stringId);
				var properties = (BaseItemProperties)entry.Get("properties");
				weaponItem.WeaponName.Text = properties.Name;

				if (_weaponController.PrimaryAttack == weapon)
					selectedIndex = i;

				weaponItem.Name = i++.ToString();
			}

			_weaponCarousel.RefreshLayout();
			_weaponCarousel.SelectIndex(selectedIndex, !countChanged);
		}
		catch (ObjectDisposedException)
		{
			// Ignore disposal during scene reload
		}
	}
}
