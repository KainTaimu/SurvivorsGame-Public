namespace SurvivorsGame.Items.Offensive;

[GlobalClass]
public partial class BaseItemProperties : Resource
{
    [Export]
    public string Name;

    [Export]
    public ItemType ItemType;

    public int CurrentLevel;

    [Export(PropertyHint.MultilineText)]
    public string Description;
}

