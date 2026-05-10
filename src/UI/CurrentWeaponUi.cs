using Game.Items.Offensive;
using Game.Players.Controllers;

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

	public override void _Ready()
	{
		_weaponController.OnPrimaryAttackReassigned += () => Callable.From(UpdateCarousel).CallDeferred();
		_weaponController.OnSecondaryAttackReassigned += () => Callable.From(UpdateCarousel).CallDeferred();
		_weaponController.OnOffensiveListChanged += _ => Callable.From(UpdateCarousel).CallDeferred();
		_weaponController.ChildOrderChanged += () => Callable.From(UpdateCarousel).CallDeferred();
		Callable.From(UpdateCarousel).CallDeferred();
	}

	public override void _Process(double delta)
	{
		// PERF: Use signals
		Callable.From(UpdateAmmoCount).CallDeferred();
	}

	public void UpdateAmmoCount()
	{
		if (_weaponController.PrimaryAttack is IReloadable primary)
		{
			_primaryWeaponAmmo.Show();
			if (primary.IsReloading)
				_primaryWeaponAmmo.Text = "Reloading...";
			else
			{
				if (primary.MagazineCount > primary.MagazineCapacity)
				{
					var extra = primary.MagazineCount - primary.MagazineCapacity;
					_primaryWeaponAmmo.Text =
						$"{primary.MagazineCount -extra}+{extra}/{primary
						.MagazineCapacity}";
				}
				else
				{
					_primaryWeaponAmmo.Text = $"{primary.MagazineCount}/{primary.MagazineCapacity}";
				}
			}
		}
		else
		{
			_primaryWeaponAmmo.Hide();
		}

		if (_weaponController.SecondaryAttack is IReloadable secondary)
		{
			_secondaryWeaponAmmo.Show();
			if (secondary.IsReloading)
				_secondaryWeaponAmmo.Text = "Reloading...";
			else
			{
				if (secondary.MagazineCount > secondary.MagazineCapacity)
				{
					var extra = secondary.MagazineCount - secondary.MagazineCapacity;
					_secondaryWeaponAmmo.Text =
						$"{secondary.MagazineCount -extra}+{extra}/{secondary
						.MagazineCapacity}";
				}
				else
				{
					_secondaryWeaponAmmo.Text = $"{secondary.MagazineCount}/{secondary.MagazineCapacity}";
				}
			}
		}
		else
		{
			_secondaryWeaponAmmo.Hide();
		}
	}

	public void UpdateCarousel()
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
			weaponItem.WeaponName.Text = weapon.Properties.Name;

			if (_weaponController.PrimaryAttack == weapon)
			{
				weaponItem.SelectedCaret.VisibleRatio = 1;
				weaponItem.SelectedCaret.Text = "1";
				weaponItem.WeaponName.LabelSettings.FontColor = Colors.White;
			}
			else if (_weaponController.SecondaryAttack == weapon)
			{
				weaponItem.SelectedCaret.VisibleRatio = 1;
				weaponItem.SelectedCaret.Text = "2";
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
