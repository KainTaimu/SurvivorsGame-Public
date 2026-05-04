namespace Game.Core;

[GlobalClass]
public partial class GameSettings : Resource
{
	[ExportGroup("DEV")]
	[Export]
	public bool EnableCrosshairHorizontalRecoilPunish = true;
}
