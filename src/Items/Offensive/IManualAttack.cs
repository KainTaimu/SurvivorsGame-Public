namespace Game.Items.Offensive;

public interface IManualAttack
{
	/// <summary>
	/// The string name of the input map action that fires this weapon.
	/// </summary>
	/// <remarks>
	/// The string should be taken from InputMapNames and not manually written
	/// </remarks>
	public string? AttackActionString { get; set; }
}
