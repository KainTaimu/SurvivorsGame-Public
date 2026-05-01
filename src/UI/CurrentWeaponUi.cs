using Game.Players.Controllers;

namespace Game.UI;

public partial class CurrentWeaponUi : CanvasLayer
{
	[Export]
	private PlayerWeaponController WeaponController = null!;

	[Export]
	private Label CurrentWeaponsLabel = null!;

	public override void _Ready()
	{
		WeaponController.OnPrimaryAttackReassigned += UpdateCurrentWeapons;
		WeaponController.OnSecondaryAttackReassigned += UpdateCurrentWeapons;
		UpdateCurrentWeapons();
	}

	public void UpdateCurrentWeapons()
	{
		var primary = WeaponController.PrimaryAttack?.GetType().Name ?? "null";
		var secondary =
			WeaponController.SecondaryAttack?.GetType().Name ?? "null";

		CurrentWeaponsLabel.Text =
			$"Primary: {primary}\nSecondary: {secondary}";
	}
}
