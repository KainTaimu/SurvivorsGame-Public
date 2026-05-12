using System.Collections.Generic;
using System.Linq;

namespace Game.Players;

[GlobalClass]
public abstract partial class AbstractStat : Resource
{
	public List<float> Multipliers = [];

	public List<float> Flat = [];

	public float GetMultipliersSum() => Multipliers.Count != 0 ? Multipliers.Sum() : 1;

	public float GetFlatSum() => Flat.Sum();

	public override string ToString()
	{
		throw new NotImplementedException();
	}
}
