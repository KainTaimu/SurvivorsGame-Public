namespace Game.Items;

[GlobalClass]
public partial class BaseItemProperties : Resource
{
	[Export]
	public string Name = "PLACEHOLDER_NAME";

	[Export]
	public ItemType ItemType;

	[Export(PropertyHint.MultilineText)]
	public string Description = _placeholderDescription;

	[Export]
	public Texture2D ItemIcon = new PlaceholderTexture2D { Size = Vector2I.One * 32 };

	public int CurrentLevel;

	private static readonly string _placeholderDescription = "PLACEHOLDER_DESCRIPTION";
}
