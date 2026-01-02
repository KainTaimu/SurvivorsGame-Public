namespace Game.Players;

public partial class Character : Node
{
    [Export]
    public string CharacterName = "";

    // CharacterStats should never be unset from its exported value!
    [Export]
    public CharacterStats CharacterStats { get; private set; } = null!;
}
