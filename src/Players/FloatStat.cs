namespace Game.Players;

[GlobalClass]
public partial class FloatStat : AbstractStat
{
	[Export]
	private float _value = 1f;

	public float Value => (_value + GetFlatSum()) * GetMultipliersSum();

	public override string ToString()
	{
		return $"{Value} ({_value} + {GetFlatSum():F2} * {GetMultipliersSum():F2})";
	}
}
