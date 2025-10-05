namespace SurvivorsGame.Items.Offensive;

[GlobalClass]
public partial class BaseItemProperties : Resource
{
    [Export] public string Name;
    [Export(PropertyHint.MultilineText)] public string Description;
    [Export] public ItemType ItemType;
    public int CurrentLevel;
}