using System.Linq;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace Game.Levels.Controllers;

[GlobalClass]
public partial class WeightedWaveBlueprintCollection : AbstractWaveBlueprintCollection
{
	[Export]
	public Array<BlueprintWeight> Weights = null!;

	public override int Count => Weights.Count;

	private float WeightsSum => Weights.Sum(x => x.Weight);

	public override EnemyBlueprint GetBlueprint()
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

		return pickedScene ?? throw new Exception("No scene picked");
	}
}
