// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Godot.Collections;

namespace Game.Levels.Controllers.Waves;

[GlobalClass]
public partial class WeightedWaveBlueprintCollection : AbstractWaveBlueprintCollection
{
	[Export]
	public Array<BlueprintWeight> Weights = [];

	public override int Count => Weights.Count;

	private float WeightsSum => Weights.Sum(x => x.Weight);

	public override EnemyBlueprint? GetBlueprint()
	{
		EnemyBlueprint? pickedScene = null;

		var pick = GD.RandRange(0, WeightsSum);
		var cum = 0f;
		foreach (var (bp, weight) in Weights.Select(x => (x.Blueprint, x.Weight)))
		{
			cum += weight;
			if (!(pick < cum))
				continue;

			pickedScene = bp;
			break;
		}

		return pickedScene;
	}
}
