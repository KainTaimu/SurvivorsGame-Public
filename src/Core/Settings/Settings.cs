using Game.Core.Settings;

namespace Game.UI;

public partial class Settings : Control
{
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

	private void UpdateOptions()
	{
		GoreSelection.Selected = GameSettings.Instance.GoreEffects switch
		{
			GoreEffectsEnum.Disabled => 0,
			GoreEffectsEnum.Low => 1,
			GoreEffectsEnum.Medium => 2,
			GoreEffectsEnum.High => 3,
			_ => throw new NotImplementedException(),
		};
		CameraShake.Selected = GameSettings.Instance.EnableCameraShake ? 1 : 0;
		CameraShakeScale.Value = GameSettings.Instance.CameraShakeScale;

		CrosshairScale.Value = GameSettings.Instance.CrosshairScale;
		CrosshairScaleLabel.Text = CrosshairScale.Value.ToString("0.##");
		DamageIndicators.Selected = GameSettings.Instance.EnableDamageIndicators
			? 1
			: 0;
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
		CameraShakeScale.ValueChanged += (value) =>
			GameSettings.Instance.CameraShakeScale = (float)value;
		CrosshairScale.ValueChanged += (value) =>
		{
			GameSettings.Instance.CrosshairScale = (float)value;
			CrosshairScaleLabel.Text = CrosshairScale.Value.ToString("0.##");
		};
		DamageIndicators.ItemSelected += (idx) =>
		{
			switch (idx)
			{
				case 0:
					GameSettings.Instance.EnableDamageIndicators = false;
					break;
				case 1:
					GameSettings.Instance.EnableDamageIndicators = true;
					break;
			}
		};
	}

	private void ResetCrosshairScale(InputEvent @event)
	{
		if (@event is not InputEventMouseButton mb)
			return;
		if (mb.ButtonIndex == MouseButton.Right)
			CrosshairScale.Value = 1.5;
	}
}
