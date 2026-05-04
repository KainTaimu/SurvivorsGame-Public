using Game.Items.Offensive;
using Game.Players.Controllers;

namespace Game.UI;

public partial class CurrentWeaponUi : CanvasLayer
{
	[Export]
	private PlayerWeaponController WeaponController = null!;

	[Export]
	private PackedScene _weaponCarouselItemScene = null!;

	[Export]
	private Control _weaponCarousel = null!;

	[Export]
	private Label _primaryWeaponAmmo = null!;

	[Export]
	private Label _secondaryWeaponAmmo = null!;

	public override void _Ready()
	{
		WeaponController.OnPrimaryAttackReassigned += () =>
			Callable.From(UpdateCarousel).CallDeferred();
		WeaponController.OnSecondaryAttackReassigned += () =>
			Callable.From(UpdateCarousel).CallDeferred();
		WeaponController.OnOffensiveListChanged += _ =>
			Callable.From(UpdateCarousel).CallDeferred();
		WeaponController.ChildOrderChanged += () =>
			Callable.From(UpdateCarousel).CallDeferred();
		Callable.From(UpdateCarousel).CallDeferred();
	}

	public override void _Process(double delta)
	{
		// PERF: Use signals
		Callable.From(UpdateAmmoCount).CallDeferred();
	}

	public void UpdateAmmoCount()
	{
		if (WeaponController.PrimaryAttack is IReloadable primary)
		{
			_primaryWeaponAmmo.Show();
			if (primary.IsReloading)
			{
				_primaryWeaponAmmo.Text = "Reloading...";
			}
			else
			{
				_primaryWeaponAmmo.Text =
					$"{primary.MagazineCount}/{primary.MagazineCapacity}";
			}
		}
		else
		{
			_primaryWeaponAmmo.Hide();
		}
		if (WeaponController.SecondaryAttack is IReloadable secondary)
		{
			_secondaryWeaponAmmo.Show();
			if (secondary.IsReloading)
			{
				_secondaryWeaponAmmo.Text = "Reloading...";
			}
			else
			{
				_secondaryWeaponAmmo.Text =
					$"{secondary.MagazineCount}/{secondary.MagazineCapacity}";
			}
		}
		else
		{
			_secondaryWeaponAmmo.Hide();
		}
	}

	public void UpdateCarousel()
	{
		var diff =
			WeaponController.Offensives.Count - _weaponCarousel.GetChildCount();
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
		foreach (var weapon in WeaponController.Offensives)
		{
			if (_weaponCarousel.GetChild(i) is not WeaponItem weaponItem)
				continue;
			weaponItem.WeaponName.Text = weapon.Properties.Name;
			if (
				WeaponController.PrimaryAttack == weapon
				|| WeaponController.SecondaryAttack == weapon
			)
			{
				weaponItem.SelectedCaret.VisibleRatio = 1;
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
}
