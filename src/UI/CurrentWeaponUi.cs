// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

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
