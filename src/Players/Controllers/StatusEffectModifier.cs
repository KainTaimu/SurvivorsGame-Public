namespace Game.Players.Controllers;

[GlobalClass]
public partial class StatusEffectModifier : Resource
{
	[Export]
	public string StatName = "";

	[Export]
	public StatusEffectModifierOperation Operation;

	[Export]
	public float Value;

	public enum StatusEffectModifierOperation
	{
		Add,
		Multiply,
	}

	public override string ToString()
	{
		var op = Operation switch
		{
			StatusEffectModifierOperation.Add => "+",
			StatusEffectModifierOperation.Multiply => "*",
			_ => throw new ArgumentOutOfRangeException(),
		};

		return $"{StatName}{op}{Value}";
	}
}
