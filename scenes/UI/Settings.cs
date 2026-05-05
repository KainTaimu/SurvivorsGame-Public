using Game.Core.Settings;

namespace Game.UI;

public partial class Settings : VBoxContainer
{
	[Export]
	public OptionButton GoreSelection = null!;

	[Export]
	public OptionButton CameraShake = null!;

	public override void _Ready()
	{
		UpdateOptions();
		SubscribeOptions();
	}

	private void UpdateOptions()
	{
		GoreSelection.Selected = GameSettings.Instance.GoreEffects switch
		{
			GoreEffectsEnum.Disabled => 0,
			GoreEffectsEnum.Low => 1,
			GoreEffectsEnum.Medium => 2,
			GoreEffectsEnum.High => 4,
			_ => throw new NotImplementedException(),
		};
		CameraShake.Selected = GameSettings.Instance.EnableCameraShake ? 1 : 0;
	}

	private void SubscribeOptions()
	{
		GoreSelection.ItemSelected += (idx) =>
		{
			switch (idx)
			{
				case 0:
					GameSettings.Instance.GoreEffects =
						GoreEffectsEnum.Disabled;
					break;
				case 1:
					GameSettings.Instance.GoreEffects = GoreEffectsEnum.Low;
					break;
				case 2:
					GameSettings.Instance.GoreEffects = GoreEffectsEnum.Medium;
					break;
				case 3:
					GameSettings.Instance.GoreEffects = GoreEffectsEnum.High;
					break;
			}
		};
		CameraShake.ItemSelected += (idx) =>
		{
			switch (idx)
			{
				case 0:
					GameSettings.Instance.EnableCameraShake = false;
					break;
				case 1:
					GameSettings.Instance.EnableCameraShake = true;
					break;
			}
		};
	}
}
