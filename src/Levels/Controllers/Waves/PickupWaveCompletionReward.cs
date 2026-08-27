namespace Game.Levels.Controllers.Waves;

[GlobalClass]
public partial class PickupWaveCompletionReward : AbstractWaveCompletionReward
{
	[Export]
	public int ItemCount = 1;

	[Export(PropertyHint.Enum, "Common,Uncommon,Rare,Epic,Legendary,Unobtainable")]
	public int RarityLowerGate;

	[Export(PropertyHint.Enum, "Common,Uncommon,Rare,Epic,Legendary,Unobtainable")]
	public int RarityUpperGate;

	[ExportGroup("Internal")]
	[Export]
	private PackedScene _pickupUiScene = GD.Load<PackedScene>("uid://c8vjoa3wtku3l");

	public override void GiveReward()
	{
		if (EnemyWaveController.Instance is null)
		{
			Logger.LogError("Tried to spawn pickup UI without wave controller.");
			return;
		}
		var pickupUi = _pickupUiScene.Instantiate();
		EnemyWaveController.Instance.AddChild(pickupUi);
		pickupUi.CallDeferred("show_ui", ItemCount, RarityLowerGate, RarityUpperGate);
	}
}
