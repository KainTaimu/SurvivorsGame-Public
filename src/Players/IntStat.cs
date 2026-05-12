namespace Game.Players;

[GlobalClass]
public partial class IntStat : AbstractStat
{
	[Export]
	private int _value;

	public int Value => Mathf.CeilToInt((_value + GetFlatSum()) * GetMultipliersSum());

	public override string ToString()
	{
		return $"{Value} ({_value} + {GetFlatSum():F2} * {GetMultipliersSum():F2})";
	}
}
