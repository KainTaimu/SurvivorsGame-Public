namespace SurvivorsGame.Items.Offensive;

[GlobalClass]
public partial class BaseItemProperties : Resource
{
    public int CurrentLevel;

    [Export(PropertyHint.MultilineText)]
    public string Description;

    [Export]
    public ItemType ItemType;

    [Export]
    public string Name;
}