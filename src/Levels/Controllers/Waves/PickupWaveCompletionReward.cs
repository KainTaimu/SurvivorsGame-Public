// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

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
			CustomLogger.LogError("Tried to spawn pickup UI without wave controller.");
			return;
		}

		var pickupUi = _pickupUiScene.Instantiate();
		EnemyWaveController.Instance.AddChild(pickupUi);
		pickupUi.CallDeferred("show_ui", ItemCount, RarityLowerGate, RarityUpperGate);
	}
}
