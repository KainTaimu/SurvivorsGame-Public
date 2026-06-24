using Game.Items;

namespace Game.UI;

public partial class ItemShowcase : MarginContainer
{
	[Signal]
	public delegate void OnItemSelectedEventHandler(PackedScene scene, BaseItemProperties properties);

	[Export]
	public RichTextLabel ItemNameLabel = null!;

	[Export]
	public RichTextLabel ItemDescriptionLabel = null!;

	[Export]
	public TextureRect ItemIconRect = null!;

	[Export]
	private PanelContainer _panel = null!;

	private bool _isMouseInside;
	private bool _itemSelected;
	private PackedScene? _itemScene;
	private BaseItemProperties? _itemProperties;

	public override void _Ready()
	{
		MouseEntered += () =>
		{
			_isMouseInside = true;
		};
		MouseExited += () =>
		{
			_isMouseInside = false;
		};
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (_itemSelected)
			return;
		if (@event is InputEventMouseMotion motion)
		{
			if (GetViewportRect().HasPoint(motion.Position))
				MouseDefaultCursorShape = CursorShape.PointingHand;
			else
				MouseDefaultCursorShape = CursorShape.Arrow;
		}

		if (!_isMouseInside)
			return;

		if (@event is not InputEventMouseButton { ButtonIndex: MouseButton.Left } mouse || !mouse.IsReleased())
			return;

		EmitSignalOnItemSelected(_itemScene, _itemProperties);
		_itemSelected = true;
	}

	public void AssignItem(PackedScene itemScene, BaseItemProperties properties, BaseItemStats stats)
	{
		ItemNameLabel.Text = "[b]" + properties.Name + "[/b]";
		ItemDescriptionLabel.Text = properties.Description + "\n\n" + stats.ToFormattedString();
		ItemIconRect.Texture = properties.ItemIcon;
		_itemScene = itemScene;
		_itemProperties = properties;
	}

	public void Reset()
	{
		_itemSelected = false;
		_itemScene = null;
		_itemProperties = null;
		Hide();
		Highlight(false);
	}

	private void Highlight(bool flag)
	{
		_panel.SelfModulate = flag ? Colors.White : Colors.Transparent;
	}
}
