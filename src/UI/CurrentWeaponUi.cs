using System.Linq;
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
	private Control _weaponCarousel = null!;

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
		_weaponController.OnPrimaryAttackReassigned += UpdateCarousel;
		_weaponController.OnSecondaryAttackReassigned += UpdateCarousel;
		_weaponController.OnOffensiveListChanged += _ => UpdateCarousel();
		_weaponController.ChildOrderChanged += UpdateCarousel;
		Callable.From(UpdateCarousel).CallDeferred();
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
			var diff = _weaponController.Offensives.Count - _weaponCarousel.GetChildCount();
			while (diff > 0)
			{
				var weaponItem = _weaponCarouselItemScene.Instantiate<WeaponItem>();
				_weaponCarousel.AddChild(weaponItem);
				diff--;
			}

			while (diff < 0)
			{
				var lastChildIndex = _weaponCarousel.GetChildCount() - 1;
				var child = _weaponCarousel.GetChild(lastChildIndex);
				_weaponCarousel.RemoveChild(child);
				child.QueueFree();
				diff++;
			}

			var i = 0;
			foreach (var weapon in _weaponController.Offensives)
			{
				if (_weaponCarousel.GetChild(i) is not WeaponItem weaponItem)
					continue;

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
				{
					// Must use VisibleRatio, Setting Visible property breaks the VBoxContainer of the WeaponItem
					weaponItem.SelectedCaret.VisibleRatio = 1;
					weaponItem.SelectedCaret.Text = "⌄";
					weaponItem.WeaponName.LabelSettings.FontColor = Colors.White;
				}
				else
				{
					weaponItem.SelectedCaret.VisibleRatio = 0;
					weaponItem.WeaponName.LabelSettings.FontColor = Colors.DarkGray;
				}

				weaponItem.Name = i++.ToString();
			}
		}
		catch (ObjectDisposedException ex)
		{
			if (ex.ObjectName != "Godot.HBoxContainer")
				throw;
		}
	}
}