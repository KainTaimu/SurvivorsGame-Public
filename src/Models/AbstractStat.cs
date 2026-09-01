using System.Linq;
using Godot.Collections;

namespace Game.Models;

[Tool]
[GlobalClass]
public abstract partial class AbstractStat : Resource
{
	[Export]
	public Array<float> Multipliers = [];

	[Export]
	public Array<float> Flat = [];

	protected AbstractStat()
	{
		ResourceLocalToScene = true;
	}

	public float GetMultipliersSum()
	{
		return Multipliers.Count != 0 ? Multipliers.Aggregate(1f, (x, y) => x * y) : 1;
	}

	public float GetFlatSum()
	{
		return Flat.Sum();
	}

	public override string ToString()
	{
		throw new NotImplementedException();
	}
}