namespace Game.Players;

[GlobalClass]
public partial class Character : Resource
{
	[Export]
	public string CharacterName = "";

	// CharacterStats should never be unset from its exported value!
	[Export]
	public CharacterStats CharacterStats { get; private set; } = null!;
}
