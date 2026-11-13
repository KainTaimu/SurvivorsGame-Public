namespace Game.Models;

[Tool]
[GlobalClass]
public partial class IntStat : AbstractStat
{
	[Export]
	public int BaseValue;

	public int Value => Mathf.CeilToInt((BaseValue + GetFlatSum()) * GetMultipliersSum());

	public override string ToString()
	{
		return $"{Value} ({BaseValue} + {GetFlatSum():F2} * {GetMultipliersSum():F2})";
	}
}
