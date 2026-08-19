using System.Collections.Generic;
using System.Linq;

namespace Game.Models;

[Tool]
[GlobalClass]
public abstract partial class AbstractStat : Resource
{
	public readonly List<float> Multipliers = [];

	public readonly List<float> Flat = [];

	protected AbstractStat()
	{
		ResourceLocalToScene = true;
	}

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
