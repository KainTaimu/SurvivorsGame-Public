using System.Collections.Generic;
using System.Linq;

namespace Game.Players;

[GlobalClass]
public abstract partial class AbstractStat : Resource
{
	public readonly List<float> Multipliers = [];

	public readonly List<float> Flat = [];

	public float GetMultipliersSum()
	{
		return Multipliers.Count != 0 ? Multipliers.Sum() : 1;
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
