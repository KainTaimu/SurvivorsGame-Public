using Game.Core.Settings;

namespace Game.UI;

public partial class Settings : Control
{
	[Export]
	public Slider MouseSensitivity = null!;

	[Export]
	public Label MouseSensitivitySideLabel = null!;

	[Export]
	public Slider MasterVolume = null!;

	[Export]
	public Label MasterVolumeSideLabel = null!;

	[Export]
	public OptionButton GoreSelection = null!;

	[Export]
	public OptionButton CameraShake = null!;

	[Export]
	public Slider CameraShakeScale = null!;

	[Export]
	public Slider CrosshairScale = null!;

	[Export]
	public Label CrosshairScaleLabel = null!;

	[Export]
	public OptionButton DamageIndicators = null!;

	public override void _Ready()
	{
		Callable
			.From(() =>
			{
				UpdateOptions();
				SubscribeOptions();
			})
			.CallDeferred();
	}

	// Try to not cause an exception in UpdateOptions and SubscribeOptions so the other settings get applied as expected

	private void UpdateOptions()
	{
		MouseSensitivity.Value = GameSettings.Instance.MouseSensitivity;
		MouseSensitivitySideLabel.Text = GameSettings.Instance.MouseSensitivity.ToString();
		MasterVolume.Value = GameSettings.Instance.MasterVolume;
		MasterVolumeSideLabel.Text = $"{GameSettings.Instance.MasterVolume}";
		switch (GameSettings.Instance.GoreEffects)
		{
			case GoreEffectsEnum.Disabled:
				GoreSelection.Selected = 0;
				break;
			case GoreEffectsEnum.Low:
				GoreSelection.Selected = 1;
				break;
			case GoreEffectsEnum.Medium:
				GoreSelection.Selected = 2;
				break;
			case GoreEffectsEnum.High:
				GoreSelection.Selected = 3;
				break;
			default:
				Logger.LogError("Invalid GoreEffects toggle enum");
				break;
		}

		CameraShake.Selected = GameSettings.Instance.EnableCameraShake ? 1 : 0;
		CameraShakeScale.Value = GameSettings.Instance.CameraShakeScale;

		CrosshairScale.Value = GameSettings.Instance.CrosshairScale;
		CrosshairScaleLabel.Text = CrosshairScale.Value.ToString("0.##");
		DamageIndicators.Selected = GameSettings.Instance.EnableDamageIndicators ? 1 : 0;
	}

	private void SubscribeOptions()
	{
		MouseSensitivity.ValueChanged += value =>
		{
			GameSettings.Instance.MouseSensitivity = (float)value;
			MouseSensitivitySideLabel.Text = $"{GameSettings.Instance.MouseSensitivity}";
		};
		MasterVolume.ValueChanged += value =>
		{
			GameSettings.Instance.MasterVolume = (float)value;
			MasterVolumeSideLabel.Text = $"{GameSettings.Instance.MasterVolume}";
		};
		GoreSelection.ItemSelected += idx =>
		{
			switch (idx)
			{
				case 0:
					GameSettings.Instance.GoreEffects = GoreEffectsEnum.Disabled;
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
				default:
					Logger.LogError("Invalid GoreEffects enum");
					break;
			}
		};
		CameraShake.ItemSelected += idx =>
		{
			switch (idx)
			{
				case 0:
					GameSettings.Instance.EnableCameraShake = false;
					break;
				case 1:
					GameSettings.Instance.EnableCameraShake = true;
					break;
				default:
					Logger.LogError("Invalid CameraShake toggle enum");
					break;
			}
		};
		CameraShakeScale.ValueChanged += value => GameSettings.Instance.CameraShakeScale = (float)value;
		CrosshairScale.ValueChanged += value =>
		{
			GameSettings.Instance.CrosshairScale = (float)value;
			CrosshairScaleLabel.Text = CrosshairScale.Value.ToString("0.##");
		};
		DamageIndicators.ItemSelected += idx =>
		{
			switch (idx)
			{
				case 0:
					GameSettings.Instance.EnableDamageIndicators = false;
					break;
				case 1:
					GameSettings.Instance.EnableDamageIndicators = true;
					break;
				default:
					Logger.LogError("Invalid DamageIndicators toggle enum");
					break;
			}
		};
	}

	// subscribe the gui_input signal from the editor to these methods
	private void ResetMouseSensitivity(InputEvent @event)
	{
		if (@event is not InputEventMouseButton mb)
			return;
		if (mb.ButtonIndex == MouseButton.Right)
			MouseSensitivity.Value = GameSettings.GetDefaultGameSettings().MouseSensitivity;
	}

	private void ResetMasterVolume(InputEvent @event)
	{
		if (@event is not InputEventMouseButton mb)
			return;
		if (mb.ButtonIndex == MouseButton.Right)
			MasterVolume.Value = GameSettings.GetDefaultGameSettings().MasterVolume;
	}

	private void ResetCrosshairScale(InputEvent @event)
	{
		if (@event is not InputEventMouseButton mb)
			return;
		if (mb.ButtonIndex == MouseButton.Right)
			CrosshairScale.Value = GameSettings.GetDefaultGameSettings().CrosshairScale;
	}
}
