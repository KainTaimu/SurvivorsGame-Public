using Game.Core;
using Game.Core.Settings;

namespace Game.UI;

public partial class Settings : VBoxContainer
{
	[Export]
	public OptionButton GoreSelection = null!;

	public GameSettings GameSettings => GameSingleton.GameSettings;

	public override void _Ready()
	{
		UpdateOptions();
		SubscribeOptions();
	}

	private void UpdateOptions()
	{
		GoreSelection.Selected = GameSettings.GoreEffects switch
		{
			GoreEffectsEnum.Disabled => 0,
			GoreEffectsEnum.Low => 1,
			GoreEffectsEnum.Medium => 2,
			GoreEffectsEnum.High => 4,
			_ => throw new NotImplementedException(),
		};
	}

	private void SubscribeOptions()
	{
		GoreSelection.ItemSelected += (idx) =>
		{
			switch (idx)
			{
				case 0:
					GameSettings.GoreEffects = GoreEffectsEnum.Disabled;
					break;
				case 1:
					GameSettings.GoreEffects = GoreEffectsEnum.Low;
					break;
				case 2:
					GameSettings.GoreEffects = GoreEffectsEnum.Medium;
					break;
				case 3:
					GameSettings.GoreEffects = GoreEffectsEnum.High;
					break;
			}
		};
	}
}
