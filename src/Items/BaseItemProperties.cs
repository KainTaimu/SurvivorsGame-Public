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

	private static readonly string _placeholderDescription = """
		Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus bibendum urna est, quis bibendum purus rhoncus at. Sed interdum massa in magna efficitur, vitae pretium nulla placerat. Praesent tincidunt in ipsum sed dictum. Fusce nec velit id mauris tincidunt facilisis vestibulum at neque. Integer eu arcu iaculis, pellentesque eros pulvinar, vulputate urna. Aliquam accumsan sapien et porttitor tempus. Fusce sollicitudin volutpat mi, quis efficitur velit aliquet volutpat. Cras et lectus tincidunt, gravida leo sed, maximus lorem.
		Ut aliquam sit amet dolor eget pulvinar. Vivamus commodo purus at purus iaculis, ut pulvinar enim faucibus. Quisque imperdiet dapibus erat, at bibendum ante vestibulum vel. Nunc molestie augue aliquam porta blandit. Aenean vehicula convallis lacus, at aliquet mi faucibus ut. Suspendisse sit amet tincidunt nunc, non tristique arcu. Aliquam dictum vestibulum augue ac vehicula. Mauris lobortis scelerisque faucibus. Etiam eu ultrices erat, eu consequat lectus.
		Etiam ornare viverra justo sed pulvinar. Aenean a congue lorem. Mauris nec nibh eget justo pulvinar bibendum id id sem. Cras et luctus enim, quis malesuada odio. Sed et nisl placerat, fermentum lorem sed, imperdiet justo. Integer eleifend, massa at blandit molestie, quam risus blandit elit, elementum porta ligula justo eu ligula. Sed suscipit, quam sed luctus tempor, mi tellus viverra leo, nec mattis eros elit eu justo. In fermentum, leo sed convallis accumsan, elit eros ultrices nulla, ut maximus tortor enim sed neque. Maecenas consectetur mi ac augue blandit, ut viverra velit ornare. Nulla consectetur metus odio, ac imperdiet orci interdum vel. Pellentesque quam neque, sollicitudin blandit auctor at, vulputate ac nisi. Praesent id urna lobortis, dapibus nibh tincidunt, venenatis felis. 
		""";
}
