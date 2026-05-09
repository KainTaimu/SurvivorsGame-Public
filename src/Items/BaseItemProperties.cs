namespace Game.Items;

[GlobalClass]
public partial class BaseItemProperties : Resource
{
	[Export]
	public string Name = "PLACEHOLDER_NAME";

	[Export]
	public ItemType ItemType;

	[Export(PropertyHint.MultilineText)]
	public string Description = "PLACEHOLDER_DESCRIPTION";

	[Export]
	public Texture2D ItemIcon = new PlaceholderTexture2D() { Size = Vector2I.One * 32 };

	public int CurrentLevel;
}
