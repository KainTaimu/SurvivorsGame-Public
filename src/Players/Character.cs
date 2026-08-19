namespace Game.Players;

[GlobalClass]
public partial class Character : Node
{
	[Export]
	public string CharacterName = "";

	[Export(PropertyHint.MultilineText)]
	public string Description = "PLACEHOLDER_DESCRIPTION";

	// CharacterStats should never be unset from its exported value!
	[Export]
	public CharacterStats CharacterStats { get; private set; } = null!;
}
